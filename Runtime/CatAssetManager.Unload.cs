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
            UnloadAsset(handler.AssetObj);
        }

        /// <summary>
        /// 卸载资源，注意：asset参数需要是原始资源对象
        /// </summary>
        internal static void UnloadAsset(object asset)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                return;
            }
#endif
            if (asset == null)
            {
                return;
            }

            AssetRuntimeInfo info = CatAssetDatabase.GetAssetRuntimeInfo(asset);

            if (info == null)
            {
                if (asset is Object unityObj)
                {
                    Debug.LogError($"要卸载的资源未加载过：{unityObj.name}，类型为{asset.GetType()}");
                }
                else
                {
                    Debug.LogError($"要卸载的资源未加载过，类型为{asset.GetType()}");
                }

                return;
            }


            InternalUnloadAsset(info);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public static void UnloadScene(SceneHandler handler)
        {
            UnloadScene(handler.Scene);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        internal static void UnloadScene(Scene scene)
        {
            if (scene == default || !scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

#if UNITY_EDITOR
            if (IsEditorMode)
            {
                SceneManager.UnloadSceneAsync(scene);
                return;
            }
#endif
            AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(scene);

            if (assetRuntimeInfo == null)
            {
                Debug.LogError($"要卸载的场景未加载过：{scene.path}");
                return;
            }

            //卸载场景
            CatAssetDatabase.RemoveSceneInstance(scene);
            SceneManager.UnloadSceneAsync(scene);

            //卸载与场景绑定的资源
            List<IBindableHandler> handlers = CatAssetDatabase.GetSceneBindAssets(scene);
            if (handlers != null)
            {
                foreach (IBindableHandler handler in handlers)
                {
                    handler.Unload();
                }
                handlers.Clear();
            }

            InternalUnloadAsset(assetRuntimeInfo);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        private static void InternalUnloadAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            //减少引用计数
            assetRuntimeInfo.SubRefCount();

            if (assetRuntimeInfo.IsUnused())
            {
                //引用计数为0
                //卸载依赖
                foreach (string dependency in assetRuntimeInfo.AssetManifest.Dependencies)
                {
                    AssetRuntimeInfo dependencyRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependency);

                    //从这里递归依赖导致的TryUnloadAssetFromMemory是一定无效的 所以会在后续UnloadAssetFromMemoryTask中重新检查一遍依赖
                    InternalUnloadAsset(dependencyRuntimeInfo);

                    //删除依赖链记录
                    dependencyRuntimeInfo.DependencyChain.DownStream.Remove(assetRuntimeInfo);
                    assetRuntimeInfo.DependencyChain.UpStream.Remove(dependencyRuntimeInfo);
                }

                TryUnloadAssetFromMemory(assetRuntimeInfo);
            }
        }

        /// <summary>
        /// 尝试将资源从内存中卸载
        /// </summary>
        internal static void TryUnloadAssetFromMemory(AssetRuntimeInfo info,bool isImmediate = false)
        {
            if (info.Asset == null)
            {
                return;
            }

            if (!info.IsUnused())
            {
                //不处理使用中的
                return;
            }

            if (info.Asset is GameObject)
            {
                //不处理GameObject
                return;
            }

            if (info.IsDownStreamInMemory())
            {
                //不处理下游资源还在内存中的 防止下游资源错误丢失依赖
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
                UnloadAssetFromMemory(info);

                //尝试将可卸载的依赖也从内存中卸载
                foreach (string dependency in info.AssetManifest.Dependencies)
                {
                    AssetRuntimeInfo dependencyRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependency);
                    TryUnloadAssetFromMemory(dependencyRuntimeInfo,true);
                }
            }

        }

        /// <summary>
        /// 将资源从内存中卸载
        /// </summary>
        internal static void UnloadAssetFromMemory(AssetRuntimeInfo info)
        {
            CatAssetDatabase.RemoveAssetInstance(info.Asset);

            BundleRuntimeInfo bundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(info.BundleManifest.RelativePath);

            if (!bundleRuntimeInfo.Manifest.IsRaw)
            {
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


            CatAssetDatabase.RemoveInMemoryAsset(info);
            info.Asset = null;
        }

        /// <summary>
        /// 立即卸载所有未使用的资源（不包括Prefab及其依赖资源）
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

                if (bundleRuntimeInfo.Manifest.IsScene)
                {
                    //跳过场景资源包
                    continue;
                }

                foreach (AssetManifestInfo assetManifestInfo in bundleRuntimeInfo.Manifest.Assets)
                {
                    AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetManifestInfo.Name);

                    TryUnloadAssetFromMemory(assetRuntimeInfo,true);
                }
            }

            Debug.Log("已卸载所有未使用的资源");
        }


        /// <summary>
        /// 添加资源包卸载任务
        /// </summary>
        internal static void AddUnloadBundleTask(BundleRuntimeInfo bundleRuntimeInfo)
        {
            UnloadBundleTask task = UnloadBundleTask.Create(unloadTaskRunner,
                bundleRuntimeInfo.Manifest.RelativePath, bundleRuntimeInfo);
            unloadTaskRunner.AddTask(task, TaskPriority.Low);
        }
    }
}
