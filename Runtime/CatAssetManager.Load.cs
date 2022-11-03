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
        public static AssetHandler<object> LoadAssetAsync(string assetName,TaskPriority priority = TaskPriority.Low)
        {
            return InternalLoadAssetAsync<object>(assetName, priority);
        }
        
        /// <summary>
        /// 加载资源
        /// </summary>
        public static AssetHandler<T> LoadAssetAsync<T>(string assetName,TaskPriority priority = TaskPriority.Low)
        {
            return InternalLoadAssetAsync<T>(assetName, priority);
        }
        
        /// <summary>
        /// 加载资源
        /// </summary>
        private static AssetHandler<T> InternalLoadAssetAsync<T>(string assetName, TaskPriority priority)
        {

            AssetHandler<T> handler;
                
            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogError("资源加载失败，资源名为空");
                handler = AssetHandler<T>.Create();
                handler.SetAssetObj(null);
                return handler;
            }
            
            Type assetType = typeof(T);

            AssetCategory category;
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                category = RuntimeUtil.GetAssetCategoryWithEditorMode(assetName,assetType);

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
                    Debug.LogError($"{e.Message}\r\n{e.StackTrace}");
                    handler = AssetHandler<T>.Create();
                    handler.SetAssetObj(null);
                    return handler;
                }


                handler = AssetHandler<T>.Create(category);
                handler.SetAssetObj(asset);
                return handler;
            }
#endif
            
            category = RuntimeUtil.GetAssetCategory(assetName);
            handler = AssetHandler<T>.Create(category);
            
            AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetName);
            if (assetRuntimeInfo.RefCount > 0)
            {
                //引用计数>0
                //直接增加引用计数
                assetRuntimeInfo.AddRefCount();
                handler.SetAssetObj(assetRuntimeInfo.Asset);
                return handler;
            }
            
            //引用计数=0 需要走一遍资源加载任务的流程
            switch (category)
            {
                case AssetCategory.None:
                    handler.SetAssetObj(null);
                    break;

                case AssetCategory.InternalBundledAsset:
                    //加载内置资源包资源
                    LoadBundledAssetTask loadBundledAssetTask =
                        LoadBundledAssetTask.Create(loadTaskRunner, assetName, assetType,handler);
                    loadTaskRunner.AddTask(loadBundledAssetTask, priority);

                    handler.Task = loadBundledAssetTask;
                    break;


                case AssetCategory.InternalRawAsset:
                case AssetCategory.ExternalRawAsset:
                    //加载原生资源
                    LoadRawAssetTask loadRawAssetTask =
                        LoadRawAssetTask.Create(loadTaskRunner, assetName, category,handler);
                    loadTaskRunner.AddTask(loadRawAssetTask, priority);

                    handler.Task = loadRawAssetTask;
                    break;
            }

            return handler;
        }

        /// <summary>
        /// 批量加载资源
        /// </summary>
        public static int BatchLoadAssetAsync(List<string> assetNames, BatchLoadAssetCallback callback,
            TaskPriority priority = TaskPriority.Low)
        {
            if (assetNames == null || assetNames.Count == 0)
            {
                Debug.LogError("批量加载资源失败，资源名列表为空");
                callback?.Invoke(null);
                return 0;
            }

#if UNITY_EDITOR
            if (IsEditorMode)
            {
                List<LoadAssetResult> assets = new List<LoadAssetResult>();
                foreach (string assetName in assetNames)
                {
                    LoadAssetAsync(assetName, ((asset, result) =>
                    {
                        assets.Add(result);

                        if (assets.Count == assetNames.Count)
                        {
                            //编辑器模式下是以同步的方式加载所有资源的 所以这里的asset顺序是和assetNames给出的顺序可以对上的
                            callback?.Invoke(assets);
                        }
                    }));
                }

                return 0;
            }
#endif

            BatchLoadAssetTask task = BatchLoadAssetTask.Create(loadTaskRunner,
                $"{nameof(BatchLoadAssetAsync)} - {TaskRunner.TaskIDFactory + 1}", assetNames, callback);
            loadTaskRunner.AddTask(task, priority);
            return task.ID;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public static int LoadSceneAsync(string sceneName, LoadSceneCallback callback,
            TaskPriority priority = TaskPriority.Low)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("场景加载失败，场景名为空");
                return 0;
            }
            
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                try
                {
                    LoadSceneParameters param = new LoadSceneParameters();
                    param.loadSceneMode = LoadSceneMode.Additive;
               
                    AsyncOperation op = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(sceneName, param);
                    op.completed += operation =>
                    {
                        Scene scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        SceneManager.SetActiveScene(scene);
                        callback?.Invoke(true, scene);
                    };
                }
                catch (Exception e)
                {
                    callback?.Invoke(false, default);
                    throw;
                }

                return 0;
            }
#endif
            if (!CheckAssetReady(sceneName))
            {
                callback?.Invoke(false, default);
                return 0;
            }

            LoadSceneTask task = LoadSceneTask.Create(loadTaskRunner, sceneName, callback);
            loadTaskRunner.AddTask(task, priority);

            return task.ID;
        }
    }
}