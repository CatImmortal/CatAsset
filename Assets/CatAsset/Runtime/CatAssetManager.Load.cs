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
        public static AssetHandler<object> LoadAssetAsync(string assetName,CancellationToken token = default, TaskPriority priority = TaskPriority.Low)
        {
            return InternalLoadAssetAsync<object>(assetName,token, priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static AssetHandler<T> LoadAssetAsync<T>(string assetName, CancellationToken token = default,TaskPriority priority = TaskPriority.Low)
        {
            return InternalLoadAssetAsync<T>(assetName,token,priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        private static AssetHandler<T> InternalLoadAssetAsync<T>(string assetName,CancellationToken token, TaskPriority priority)
        {
            
            AssetHandler<T> handler;

            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogError("资源加载失败：资源名为空");
                handler = AssetHandler<T>.Create();
                handler.SetAsset(null);
                return handler;
            }

            Type assetType = typeof(T);
            AssetCategory category;
            
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                category = RuntimeUtil.GetAssetCategoryWithEditorMode(assetName, assetType);
                handler = AssetHandler<T>.Create(assetName,token, category);
                
                object asset;
                
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

                if (asset == null)
                {
                    Debug.LogError($"资源加载失败：{assetName}");
                }
                
                handler.SetAsset(asset);
                return handler;
            }
#endif

            category = RuntimeUtil.GetAssetCategory(assetName);
            handler = AssetHandler<T>.Create(assetName,token, category);

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
        public static BatchAssetHandler BatchLoadAssetAsync(List<string> assetNames,CancellationToken token = default, TaskPriority priority = TaskPriority.Low)
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
        public static SceneHandler LoadSceneAsync(string sceneName,CancellationToken token = default, TaskPriority priority = TaskPriority.Low)
        {
            SceneHandler handler = SceneHandler.Create(sceneName,token);

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
                LoadSceneParameters param = new LoadSceneParameters
                {
                    loadSceneMode = LoadSceneMode.Additive
                };

                Scene scene = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(sceneName, param);

                if (scene == default)
                {
                    Debug.LogError($"场景加载失败：{sceneName}");
                }
                else
                {
                    SceneManager.SetActiveScene(scene);
                }
                handler.SetScene(scene);

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

        /// <summary>
        /// 加载预制体并实例化，会自行将Handler绑定至实例化出的游戏物体上
        /// </summary>
        public static void InstantiateAsync(string prefabName,Action<GameObject> callback,Transform parent = null)
        {
            LoadAssetAsync<GameObject>(prefabName).OnLoaded += handler =>
            {
                GameObject go = null;
                if (handler.State == HandlerState.Success)
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