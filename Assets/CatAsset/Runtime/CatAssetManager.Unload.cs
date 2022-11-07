using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 卸载资源，注意：asset参数需要是原始资源实例
        /// </summary>
        public static void UnloadAsset(object asset)
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
        public static void UnloadScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
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
            List<AssetHandler> handlers = CatAssetDatabase.GetSceneBindAssets(scene);
            if (handlers != null)
            {
                foreach (AssetHandler handler in handlers)
                {
                    handler.Dispose();
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
                    InternalUnloadAsset(dependencyRuntimeInfo);
                    
                    //删除依赖链记录
                    dependencyRuntimeInfo.RemoveDownStream(assetRuntimeInfo);
                }

                //对于非场景 非Prefab的资源 以及原生资源 创建卸载任务
                BundleRuntimeInfo bundleRuntimeInfo =
                    CatAssetDatabase.GetBundleRuntimeInfo(assetRuntimeInfo.BundleManifest.RelativePath);
                if (!bundleRuntimeInfo.Manifest.IsScene && !(assetRuntimeInfo.Asset is GameObject))
                {
                    UnloadAssetTask task = UnloadAssetTask.Create(unloadTaskRunner,assetRuntimeInfo.AssetManifest.Name,assetRuntimeInfo);
                    unloadTaskRunner.AddTask(task,TaskPriority.Low);
                }
            }
        }

        /// <summary>
        /// 卸载所有未使用的资源，若isQuick为true则是不处理Prefab的快速模式
        /// </summary>
        public static void UnloadUnusedAssets(bool isQuickMode = true)
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

                    if (assetRuntimeInfo.Asset == null || assetRuntimeInfo.RefCount > 0)
                    {
                        //资源未加载 或 引用计数>0 跳过
                        continue;
                    }

                    if (!isQuickMode)
                    {
                        //非快速模式下 只解除引用 不调用Resources.UnloadAsset 等待后面的Resources.UnloadUnusedAssets来卸载
                        CatAssetDatabase.RemoveAssetInstance(assetRuntimeInfo.Asset);
                        assetRuntimeInfo.Asset = null;
                        continue;
                    }
                  
                    //快速模式下 只处理非Prefab资源
                    if (!(assetRuntimeInfo.Asset is GameObject))
                    {
                        CatAssetDatabase.RemoveAssetInstance(assetRuntimeInfo.Asset);
                        if (assetRuntimeInfo.Asset is Object unityObj)
                        {
                            UnloadAssetFromMemory(unityObj);
                        }
                        assetRuntimeInfo.Asset = null;
                    }

                    
                }
            }

            if (!isQuickMode)
            {
                Resources.UnloadUnusedAssets();
            }
            
            Debug.Log("已卸载所有未使用的资源");
        }

        /// <summary>
        /// 将资源包资源从内存卸载
        /// </summary>
        internal static void UnloadAssetFromMemory(Object asset)
        {
            if (asset is Sprite sprite)
            {
                //注意 sprite资源得卸载它的texture
                Resources.UnloadAsset(sprite.texture);
            }
            else
            {
                Resources.UnloadAsset(asset);
            }
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