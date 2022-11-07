using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 检查资源是否已准备好
        /// </summary>
        private static bool CheckAssetReady(string assetName)
        {
            AssetRuntimeInfo info = CatAssetDatabase.GetAssetRuntimeInfo(assetName);
            if (info == null)
            {
                Debug.LogError($"资源加载失败，不在资源清单中：{assetName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static AssetHandler<object> LoadAssetAsync(string assetName,AssetLoadedCallback<object> callback = null, TaskPriority priority = TaskPriority.Low)
        {
            return InternalLoadAssetAsync<object>(assetName,callback, priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static AssetHandler<T> LoadAssetAsync<T>(string assetName,AssetLoadedCallback<T> callback = null, TaskPriority priority = TaskPriority.Low)
        {
            return InternalLoadAssetAsync<T>(assetName,callback,priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        private static AssetHandler<T> InternalLoadAssetAsync<T>(string assetName,AssetLoadedCallback<T> callback, TaskPriority priority)
        {
            
            AssetHandler<T> handler;

            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogError("资源加载失败：资源名为空");
                handler = AssetHandler<T>.Create(callback);
                handler.SetAsset(null);
                return handler;
            }

            Type assetType = typeof(T);
            AssetCategory category;
            
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                category = RuntimeUtil.GetAssetCategoryWithEditorMode(assetName, assetType);
                handler = AssetHandler<T>.Create(callback,category);
                
                object asset;
                try
                {
                    if (category == AssetCategory.InternalBundledAsset)
                    {
                        //加载资源包资源
                        asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, assetType);
                    }
                    else
                    {
                        //加载原生资源
                        if (category == AssetCategory.ExternalRawAsset)
                        {
                            assetName = RuntimeUtil.GetReadWritePath(assetName);
                        }

                        asset = File.ReadAllBytes(assetName);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"资源加载失败：{e.Message}\r\n{e.StackTrace}");
                    handler.SetAsset(null);
                    return handler;
                }
                
                handler.SetAsset(asset);
                return handler;
            }
#endif

            category = RuntimeUtil.GetAssetCategory(assetName);
            handler = AssetHandler<T>.Create(callback,category);

            AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetName);
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
                    handler.SetAsset(null);
                    break;

                case AssetCategory.InternalBundledAsset:
                    //加载内置资源包资源
                    LoadBundledAssetTask loadBundledAssetTask =
                        LoadBundledAssetTask.Create(loadTaskRunner, assetName, assetType, handler);
                    loadTaskRunner.AddTask(loadBundledAssetTask, priority);

                    handler.Task = loadBundledAssetTask;
                    break;


                case AssetCategory.InternalRawAsset:
                case AssetCategory.ExternalRawAsset:
                    //加载原生资源
                    LoadRawAssetTask loadRawAssetTask =
                        LoadRawAssetTask.Create(loadTaskRunner, assetName, category, handler);
                    loadTaskRunner.AddTask(loadRawAssetTask, priority);

                    handler.Task = loadRawAssetTask;
                    break;
            }

            return handler;
        }

        /// <summary>
        /// 批量加载资源
        /// </summary>
        public static BatchAssetHandler BatchLoadAssetAsync(List<string> assetNames,BatchAssetLoadedCallback callback = null,
            TaskPriority priority = TaskPriority.Low)
        {
            BatchAssetHandler handler;

            if (assetNames == null || assetNames.Count == 0)
            {
                Debug.LogWarning("批量加载资源的资源名列表为空");
                handler = BatchAssetHandler.Create(0,callback);
                return handler;
            }

            handler = BatchAssetHandler.Create(assetNames.Count,callback);

            foreach (string assetName in assetNames)
            {
                AssetHandler<object> assetHandler = LoadAssetAsync(assetName,handler.OnAssetLoadedCallback);
                handler.AddAssetHandler(assetHandler);
            }

            return handler;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public static SceneHandler LoadSceneAsync(string sceneName,SceneLoadedCallback callback = null, TaskPriority priority = TaskPriority.Low)
        {
            SceneHandler handler = SceneHandler.Create(callback);

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("场景加载失败：场景名为空");
                handler.SetScene(default);
                return handler;
            }

            InternalLoadSceneAsync(sceneName,handler,priority);
            return handler;
        }

        /// <summary>
        /// 加载场景，使用外部传入的SceneHandler
        /// </summary>
        internal static void InternalLoadSceneAsync(string sceneName, SceneHandler handler,
            TaskPriority priority = TaskPriority.Low)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                try
                {
                    LoadSceneParameters param = new LoadSceneParameters();
                    param.loadSceneMode = LoadSceneMode.Additive;

                    AsyncOperation op =
                        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(sceneName, param);
                    op.completed += operation =>
                    {
                        Scene scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        SceneManager.SetActiveScene(scene);
                        handler.SetScene(scene);
                    };
                }
                catch (Exception e)
                {
                    Debug.LogError($"场景加载失败：{e.Message}\r\n{e.StackTrace}");
                    handler.SetScene(default);
                    return;
                }

                return;
            }
#endif
            if (!CheckAssetReady(sceneName))
            {
                handler.SetScene(default);
                return;
            }

            LoadSceneTask task = LoadSceneTask.Create(loadTaskRunner, sceneName, handler);
            loadTaskRunner.AddTask(task, priority);

            handler.Task = task;
        }
    }
}