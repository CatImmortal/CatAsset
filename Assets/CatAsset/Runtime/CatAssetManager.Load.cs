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
        public static int LoadAssetAsync(string assetName,LoadAssetCallback<object> callback,
            TaskPriority priority = TaskPriority.Low)
        {
            int id = InternalLoadAssetAsync(assetName, typeof(object), callback, ((userdata, result) =>
            {
                LoadAssetCallback<object> localCallback = (LoadAssetCallback<object>)userdata;
                localCallback?.Invoke(result.Asset,result);
            }),priority);
            return id;
        }
        
        /// <summary>
        /// 加载资源
        /// </summary>
        public static int LoadAssetAsync(string assetName,Type assetType,LoadAssetCallback<object> callback,
            TaskPriority priority = TaskPriority.Low)
        {
            int id = InternalLoadAssetAsync(assetName, assetType, callback, ((userdata, result) =>
            {
                LoadAssetCallback<object> localCallback = (LoadAssetCallback<object>)userdata;
                localCallback?.Invoke(result.Asset,result);
            }),priority);
            return id;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static int LoadAssetAsync<T>(string assetName,LoadAssetCallback<T> callback,
            TaskPriority priority = TaskPriority.Low)
        {
            int id = InternalLoadAssetAsync(assetName, typeof(T), callback, ((userdata, result) =>
            {
                LoadAssetCallback<T> localCallback = (LoadAssetCallback<T>)userdata;
                localCallback?.Invoke(result.GetAsset<T>(),result);
            }),priority);
            return id;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        private static int InternalLoadAssetAsync(string assetName, Type assetType,object userdata, InternalLoadAssetCallback callback,
            TaskPriority priority)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogError("资源加载失败，资源名为空");
                return 0;
            }

            if (assetType == null)
            {
                Debug.LogError("资源加载失败，资源类型为空");
                return 0;
            }
            
            AssetCategory category;
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                category = Util.GetAssetCategoryWithEditorMode(assetName,assetType);

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
                            assetName = Util.GetReadWritePath(assetName);
                        }

                        asset = File.ReadAllBytes(assetName);
                    }
                }
                catch (Exception e)
                {
                    callback?.Invoke(userdata, default);
                    throw;
                }

                LoadAssetResult result = new LoadAssetResult(asset, category);
                callback?.Invoke(userdata, result);
                return 0;
            }
#endif
            
            category = Util.GetAssetCategory(assetName);
            
            AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetName);
            if (assetRuntimeInfo.RefCount > 0)
            {
                //引用计数>0
                //直接增加引用计数 通知回调
                assetRuntimeInfo.AddRefCount();
                
                LoadAssetResult result = new LoadAssetResult(assetRuntimeInfo.Asset, category);
                callback?.Invoke(userdata,result);
                
                return 0;
            }
            
            //引用计数=0 需要走一遍资源加载任务的流程
            switch (category)
            {
                case AssetCategory.None:
                    callback?.Invoke(userdata, default);
                    return default;

                case AssetCategory.InternalBundledAsset:
                    //加载内置资源包资源
                    LoadBundledAssetTask loadBundledAssetTask =
                        LoadBundledAssetTask.Create(loadTaskRunner, assetName, assetType,userdata, callback);
                    loadTaskRunner.AddTask(loadBundledAssetTask, priority);
                    return loadBundledAssetTask.ID;


                case AssetCategory.InternalRawAsset:
                case AssetCategory.ExternalRawAsset:
                    //加载原生资源
                    LoadRawAssetTask loadRawAssetTask =
                        LoadRawAssetTask.Create(loadTaskRunner, assetName, category,userdata,callback);
                    loadTaskRunner.AddTask(loadRawAssetTask, priority);

                    return loadRawAssetTask.ID;
            }

            return default;
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