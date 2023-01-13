using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace CatAsset.Runtime
{
    /// <summary>
    /// 默认资源加载器
    /// </summary>
    public abstract class DefaultAssetLoader : BaseAssetLoader
    {
        /// <inheritdoc />
        protected override AssetHandler<T> InternalLoadAssetAsync<T>(string assetName, CancellationToken token,
            TaskPriority priority)
        {
            AssetHandler<T> handler;

            if (string.IsNullOrEmpty(assetName))
            {
                handler = AssetHandler<T>.Create();
                handler.Error = "资源名为空";
                handler.SetAsset(null);
                return handler;
            }

            Type assetType = typeof(T);
            AssetCategory category;
            
            category = RuntimeUtil.GetAssetCategory(assetName);
            handler = AssetHandler<T>.Create(assetName,token, category);

            AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetName);
            if (assetRuntimeInfo == null)
            {
                handler.Error = "未获取到AssetRuntimeInfo，请检查资源名是否正确";
                handler.SetAsset(null);
                return handler;
            }
            
            if (assetRuntimeInfo.RefCount > 0)
            {
                //引用计数>0
                //直接增加引用计数
                assetRuntimeInfo.AddRefCount();
                handler.SetAsset(assetRuntimeInfo.Asset);
                return handler;
            }

            //引用计数=0 需要走一遍资源加载任务的流程
            switch (category)
            {
                case AssetCategory.None:
                    handler.Error = "AssetCategory为None，请检查资源名是否正确";
                    handler.SetAsset(null);
                    break;

                case AssetCategory.InternalBundledAsset:
                    //加载内置资源包资源
                    CatAssetManager.AddLoadBundledAssetTask(assetName,assetType,handler,priority);
                    break;


                case AssetCategory.InternalRawAsset:
                case AssetCategory.ExternalRawAsset:
                    //加载原生资源
                   CatAssetManager.AddLoadRawAssetTask(assetName,category,handler,priority);
                    break;
            }

            return handler;
        }


        /// <inheritdoc />
        internal override void InternalLoadSceneAsync(string sceneName, SceneHandler handler, TaskPriority priority = TaskPriority.Low)
        {
            AssetRuntimeInfo info = CatAssetDatabase.GetAssetRuntimeInfo(sceneName);
            if (info == null)
            {
                handler.Error = "未获取到AssetRuntimeInfo，请检查场景名是否正确";
                handler.SetScene(default);
                return;
            }

            CatAssetManager.AddLoadSceneTask(sceneName,handler,priority);
        }

        /// <inheritdoc />
        public override void UnloadAsset(object asset)
        {
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

        /// <inheritdoc />
        public override void UnloadScene(Scene scene)
        {
            if (scene == default || !scene.IsValid() || !scene.isLoaded)
            {
                return;
            }
            
            AssetRuntimeInfo info = CatAssetDatabase.GetAssetRuntimeInfo(scene);

            if (info == null)
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
                foreach (IBindableHandler bindable in handlers)
                {
                    bindable.Unload();
                }
                handlers.Clear();
            }

            InternalUnloadAsset(info);
        }
        
        /// <summary>
        /// 卸载资源
        /// </summary>
        private void InternalUnloadAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            //减少引用计数
            assetRuntimeInfo.SubRefCount();

            if (assetRuntimeInfo.IsUnused())
            {
                //引用计数为0
                foreach (string dependency in assetRuntimeInfo.AssetManifest.Dependencies)
                {
                    AssetRuntimeInfo dependencyRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependency);

                    //删除依赖链记录
                    dependencyRuntimeInfo.DependencyChain.DownStream.Remove(assetRuntimeInfo);
                    assetRuntimeInfo.DependencyChain.UpStream.Remove(dependencyRuntimeInfo);

                    //递归卸载依赖
                    InternalUnloadAsset(dependencyRuntimeInfo);
                }
            }
        }
    }
}