using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 卸载资源
        /// </summary>
        public static void UnloadAsset(AssetHandler handler)
        {
            assetLoader.UnloadAsset(handler.AssetObj);
        }

        /// <summary>
        /// 卸载资源，注意：asset参数需要是原始资源对象
        /// </summary>
        internal static void UnloadAsset(object asset)
        {
            assetLoader.UnloadAsset(asset);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public static void UnloadScene(SceneHandler handler)
        {
            assetLoader.UnloadScene(handler);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        internal static void UnloadScene(Scene scene)
        {
            assetLoader.UnloadScene(scene);
        }
        
        /// <summary>
        /// 尝试将资源从内存中卸载
        /// </summary>
        internal static void TryUnloadAssetFromMemory(AssetRuntimeInfo info,bool isImmediate = false)
        {
            if (!info.CanUnload())
            {
                return;
            }

            if (!isImmediate)
            {
                //不立即卸载 创建卸载任务
                UnloadAssetFromMemoryTask task = UnloadAssetFromMemoryTask.Create(unloadTaskRunner,info.AssetManifest.Name,info);
                unloadTaskRunner.AddTask(task,TaskPriority.Low);
            }
            else
            {
                //立即卸载
                UnloadAssetFromMemory(info,true);
            }

        }

        /// <summary>
        /// 将资源从内存中卸载
        /// </summary>
        internal static void UnloadAssetFromMemory(AssetRuntimeInfo info,bool isImmediate = false)
        {
            CatAssetDatabase.RemoveAssetInstance(info.Asset);

            BundleRuntimeInfo bundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(info.BundleManifest.BundleIdentifyName);

            if (!bundleRuntimeInfo.Manifest.IsRaw)
            {
                //资源包资源 使用Resources.UnloadAsset卸载
                Object unityObj = (Object)info.Asset;
                if (unityObj is Sprite sprite)
                {
                    //Sprite得卸载它的texture
                    Resources.UnloadAsset(sprite.texture);
                }
                else
                {
                    Resources.UnloadAsset(unityObj);
                }
            }

            //置空资源引用 清空依赖链上游
            info.Asset = null;
            info.ClearDependencyChainUpStream();

            Debug.Log($"已卸载资源:{info}");

            //尝试将可卸载的依赖也从内存中卸载
            foreach (string dependency in info.AssetManifest.Dependencies)
            {
                AssetRuntimeInfo dependencyRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependency);
                TryUnloadAssetFromMemory(dependencyRuntimeInfo,isImmediate);
            }
        }

        /// <summary>
        /// 尝试将资源包从内存中卸载
        /// </summary>
        internal static void TryUnloadBundle(BundleRuntimeInfo info,bool isImmediate = false)
        {
            if (!info.CanUnload())
            {
                return;
            }

            if (!isImmediate)
            {
                //不立即卸载 创建卸载任务
                UnloadBundleTask task = UnloadBundleTask.Create(unloadTaskRunner,
                    info.Manifest.BundleIdentifyName, info);
                unloadTaskRunner.AddTask(task, TaskPriority.Low);
            }
            else
            {
                //立即卸载
                UnloadBundle(info,true);
            }
        }

        /// <summary>
        /// 将资源包从内存中卸载
        /// </summary>
        internal static void UnloadBundle(BundleRuntimeInfo bundleRuntimeInfo,bool isImmediate = false)
        {
            foreach (AssetManifestInfo assetManifestInfo in bundleRuntimeInfo.Manifest.Assets)
            {
                AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetManifestInfo.Name);

                if (assetRuntimeInfo.Asset != null)
                {
                    //对于此时还在内存中的资源
                    //置空资源引用 清空依赖链上游
                    CatAssetDatabase.RemoveAssetInstance(assetRuntimeInfo.Asset);
                    assetRuntimeInfo.Asset = null;
                    assetRuntimeInfo.ClearDependencyChainUpStream();
                    
                    //尝试将可卸载的依赖从内存中卸载
                    foreach (string dependency in assetRuntimeInfo.AssetManifest.Dependencies)
                    {
                        AssetRuntimeInfo dependencyRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependency);
                        TryUnloadAssetFromMemory(dependencyRuntimeInfo,isImmediate);
                    }
                }
            }

            //删除此资源包在依赖链上的信息
            //并尝试卸载上游资源包
            foreach (BundleRuntimeInfo upStreamBundle in bundleRuntimeInfo.DependencyChain.UpStream)
            {
                upStreamBundle.DependencyChain.DownStream.Remove(bundleRuntimeInfo);
                TryUnloadBundle(upStreamBundle,isImmediate);
            }
            bundleRuntimeInfo.DependencyChain.UpStream.Clear();

            //卸载资源包
            bundleRuntimeInfo.Bundle.Unload(true);
            bundleRuntimeInfo.Bundle = null;

            Debug.Log($"已卸载资源包:{bundleRuntimeInfo.Manifest.BundleIdentifyName}");
        }

        /// <summary>
        /// 立即卸载所有未使用的资源与资源包
        /// </summary>
        public static void UnloadUnusedAssets()
        {
            foreach (KeyValuePair<string,BundleRuntimeInfo> pair in CatAssetDatabase.GetAllBundleRuntimeInfo())
            {
                BundleRuntimeInfo bundleRuntimeInfo = pair.Value;

                if (!bundleRuntimeInfo.Manifest.IsRaw && bundleRuntimeInfo.Bundle == null)
                {
                    //跳过未加载的资源包
                    continue;
                }

                foreach (AssetManifestInfo assetManifestInfo in bundleRuntimeInfo.Manifest.Assets)
                {
                    //尝试立即卸载未使用的资源
                    AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetManifestInfo.Name);
                    TryUnloadAssetFromMemory(assetRuntimeInfo,true);
                }

                //尝试立即卸载未使用的资源包
                TryUnloadBundle(bundleRuntimeInfo,true);

            }

            Debug.Log("已卸载所有未使用的资源与资源包");
        }
    }
}
