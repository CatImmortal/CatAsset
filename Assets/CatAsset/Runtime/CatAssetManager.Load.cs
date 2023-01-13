using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 添加加载场景的任务
        /// </summary>
        public static void AddLoadSceneTask(string sceneName,SceneHandler handler,TaskPriority priority)
        {
            LoadSceneTask task = LoadSceneTask.Create(loadTaskRunner, sceneName, handler);
            loadTaskRunner.AddTask(task, priority);

            handler.Task = task;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static AssetHandler<object> LoadAssetAsync(string assetName,CancellationToken token = default, TaskPriority priority = TaskPriority.Low)
        {
            return assetLoader.LoadAssetAsync(assetName, token, priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static AssetHandler<T> LoadAssetAsync<T>(string assetName, CancellationToken token = default,TaskPriority priority = TaskPriority.Low)
        {
            return assetLoader.LoadAssetAsync<T>(assetName, token, priority);
        }


        /// <summary>
        /// 批量加载资源
        /// </summary>
        public static BatchAssetHandler BatchLoadAssetAsync(List<string> assetNames,CancellationToken token = default, TaskPriority priority = TaskPriority.Low)
        {
            return assetLoader.BatchLoadAssetAsync(assetNames, token, priority);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public static SceneHandler LoadSceneAsync(string sceneName,CancellationToken token = default, TaskPriority priority = TaskPriority.Low)
        {
            return assetLoader.LoadSceneAsync(sceneName, token, priority);
        }

        /// <summary>
        /// 加载场景，使用外部传入的SceneHandler
        /// </summary>
        internal static void InternalLoadSceneAsync(string sceneName, SceneHandler handler,
            TaskPriority priority = TaskPriority.Low)
        {
            assetLoader.InternalLoadSceneAsync(sceneName, handler, priority);
        }

        /// <summary>
        /// 加载预制体并实例化，会自行将Handler绑定至实例化出的游戏物体上
        /// </summary>
        public static void InstantiateAsync(string prefabName,Action<GameObject> callback,Transform parent = null)
        {
            LoadAssetAsync<GameObject>(prefabName).OnLoaded += handler =>
            {
                GameObject go = null;
                if (handler.IsSuccess)
                {
                    go = Object.Instantiate(handler.Asset,parent);
                    go.BindTo(handler);
                }
                else
                {
                    handler.Unload();
                }
        
                callback?.Invoke(go);
            };
        }
    }
}
