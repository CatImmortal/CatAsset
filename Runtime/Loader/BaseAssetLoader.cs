using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源加载器基类
    /// </summary>
    public abstract class BaseAssetLoader
    {
        /// <summary>
        /// 检查资源版本
        /// </summary>
        public abstract void CheckVersion(OnVersionChecked onVersionChecked);

        /// <summary>
        /// 加载资源
        /// </summary>
        public virtual AssetHandler<object> LoadAssetAsync(string assetName, CancellationToken token = default,
            TaskPriority priority = TaskPriority.Low)
        {
            return InternalLoadAssetAsync<object>(assetName,token, priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public virtual AssetHandler<T> LoadAssetAsync<T>(string assetName, CancellationToken token = default,
            TaskPriority priority = TaskPriority.Low)
        {
            return InternalLoadAssetAsync<T>(assetName,token,priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        protected abstract AssetHandler<T> InternalLoadAssetAsync<T>(string assetName, CancellationToken token,
            TaskPriority priority);

        /// <summary>
        /// 批量加载资源
        /// </summary>
        public virtual BatchAssetHandler BatchLoadAssetAsync(List<string> assetNames, CancellationToken token = default,
            TaskPriority priority = TaskPriority.Low)
        {
            BatchAssetHandler handler;

            if (assetNames == null || assetNames.Count == 0)
            {
                Debug.LogWarning("批量加载资源的资源名列表为空");
                handler = BatchAssetHandler.Create();
                return handler;
            }

            handler = BatchAssetHandler.Create(assetNames.Count,token);
            foreach (string assetName in assetNames)
            {
                AssetHandler<object> assetHandler = LoadAssetAsync(assetName);
                assetHandler.OnLoaded += handler.OnAssetLoadedCallback;
                handler.AddAssetHandler(assetHandler);
            }
            handler.CheckLoaded();

            return handler;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public virtual SceneHandler LoadSceneAsync(string sceneName, CancellationToken token = default,
            TaskPriority priority = TaskPriority.Low)
        {
            SceneHandler handler = SceneHandler.Create(sceneName,token);

            if (string.IsNullOrEmpty(sceneName))
            {
                handler.Error = "场景名为空";
                handler.SetScene(default);
                return handler;
            }

            InternalLoadSceneAsync(sceneName,handler,priority);
            return handler;
        }

        /// <summary>
        /// 加载场景，使用已有的SceneHandler
        /// </summary>
        internal abstract void InternalLoadSceneAsync(string sceneName, SceneHandler handler,
            TaskPriority priority = TaskPriority.Low);

        /// <summary>
        /// 卸载资源
        /// </summary>
        public virtual void UnloadAsset(AssetHandler handler)
        {
            UnloadAsset(handler.AssetObj);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public abstract void UnloadAsset(object asset);

        /// <summary>
        /// 卸载场景
        /// </summary>
        public virtual void UnloadScene(SceneHandler handler)
        {
            UnloadScene(handler.Scene);
        }
        
        /// <summary>
        /// 卸载场景
        /// </summary>
        public abstract void UnloadScene(Scene scene);
    }
}