using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源管理器
    /// </summary>
    public static class CatAssetManager
    {
        /// <summary>
        /// 加载相关任务运行器
        /// </summary>
        private static TaskRunner loadTaskRunner = new TaskRunner();

        /// <summary>
        /// 下载相关任务运行器
        /// </summary>
        private static TaskRunner downloadTaskRunner = new TaskRunner();

        /// <summary>
        /// 资源包相对路径->资源包运行时信息（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        private static Dictionary<string, BundleRuntimeInfo> bundleRuntimeInfoDict =
            new Dictionary<string, BundleRuntimeInfo>();

        /// <summary>
        /// 资源名->资源运行时信息（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        private static Dictionary<string, AssetRuntimeInfo> assetRuntimeInfoDict =
            new Dictionary<string, AssetRuntimeInfo>();

        /// <summary>
        /// 资源实例->资源运行时信息
        /// </summary>
        private static Dictionary<object, AssetRuntimeInfo> assetInstanceDict =
            new Dictionary<object, AssetRuntimeInfo>();

        /// <summary>
        /// 场景实例handler->资源运行时信息
        /// </summary>
        private static Dictionary<int, AssetRuntimeInfo> sceneInstanceDict = new Dictionary<int, AssetRuntimeInfo>();

        /// <summary>
        /// 任务id->任务
        /// </summary>
        private static Dictionary<int, ITask> allTaskDict = new Dictionary<int, ITask>();

        /// <summary>
        /// 运行模式
        /// </summary>
        public static RuntimeMode RuntimeMode { get; set; }

        /// <summary>
        /// 是否开启编辑器资源模式
        /// </summary>
        public static bool IsEditorMode { get; set; }

        /// <summary>
        /// 资源包卸载延迟时间
        /// </summary>
        public static float UnloadDelayTime { get; set; }

        
        /// <summary>
        /// 轮询CatAsset资源管理器
        /// </summary>
        public static void Update()
        {
            loadTaskRunner.Update();
            downloadTaskRunner.Update();
        }
        
        #region 数据操作

         /// <summary>
        /// 根据资源包清单信息初始化运行时信息
        /// </summary>
        private static void InitRuntimeInfo(BundleManifestInfo bundleManifestInfo, bool inReadWrite)
        {
            BundleRuntimeInfo bundleRuntimeInfo = new BundleRuntimeInfo();
            bundleRuntimeInfoDict.Add(bundleManifestInfo.RelativePath, bundleRuntimeInfo);
            bundleRuntimeInfo.Manifest = bundleManifestInfo;
            bundleRuntimeInfo.InReadWrite = inReadWrite;

            foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
            {
                AssetRuntimeInfo assetRuntimeInfo = new AssetRuntimeInfo();
                assetRuntimeInfoDict.Add(assetManifestInfo.Name, assetRuntimeInfo);
                assetRuntimeInfo.BundleManifest = bundleManifestInfo;
                assetRuntimeInfo.AssetManifest = assetManifestInfo;
            }
        }

        /// <summary>
        /// 获取资源包运行时信息
        /// </summary>
        internal static BundleRuntimeInfo GetBundleRuntimeInfo(string bundleRelativePath)
        {
            return bundleRuntimeInfoDict[bundleRelativePath];
        }

        /// <summary>
        /// 获取资源运行时信息
        /// </summary>
        internal static AssetRuntimeInfo GetAssetRuntimeInfo(string assetName)
        {
            return assetRuntimeInfoDict[assetName];
        }

        /// <summary>
        /// 获取资源运行时信息
        /// </summary>
        internal static AssetRuntimeInfo GetAssetRuntimeInfo(object asset)
        {
            return assetInstanceDict[asset];
        }

        /// <summary>
        /// 获取场景运行时信息
        /// </summary>
        internal static AssetRuntimeInfo GetAssetRuntimeInfo(Scene scene)
        {
            return sceneInstanceDict[scene.handle];
        }

        /// <summary>
        /// 检查资源是否已准备好
        /// </summary>
        private static bool CheckAssetReady(string assetName)
        {
            if (!assetRuntimeInfoDict.ContainsKey(assetName))
            {
                Debug.LogError($"资源加载失败，不在资源清单中：{assetName}");
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// 设置资源实例与资源运行时信息的关联
        /// </summary>
        internal static void SetAssetInstance(object asset, AssetRuntimeInfo assetRuntimeInfo)
        {
            assetInstanceDict.Add(asset, assetRuntimeInfo);
        }

        /// <summary>
        /// 删除资源实例与资源运行时信息的关联
        /// </summary>
        internal static void RemoveAssetInstance(object asset)
        {
            assetInstanceDict.Remove(asset);
        }

        /// <summary>
        /// 设置场景实例与资源运行时信息的关联
        /// </summary>
        internal static void SetSceneInstance(Scene scene, AssetRuntimeInfo assetRuntimeInfo)
        {
            sceneInstanceDict.Add(scene.handle, assetRuntimeInfo);
        }

        /// <summary>
        /// 删除场景实例与资源运行时信息的关联
        /// </summary>
        internal static void RemoveSceneInstance(Scene scene)
        {
            sceneInstanceDict.Remove(scene.handle);
        }

        /// <summary>
        /// 添加任务id与任务的关联
        /// </summary>
        internal static void AddTaskGUID(ITask task)
        {
            allTaskDict.Add(task.GUID,task);
        }
        
        /// <summary>
        /// 删除任务id与任务的关联
        /// </summary>
        internal static void RemoveTaskGUID(ITask task)
        {
            allTaskDict.Remove(task.GUID);
        }
        
        #endregion

        #region 资源清单检查

        /// <summary>
        /// 检查安装包内资源清单,仅使用安装包内资源模式下专用
        /// </summary>
        public static void CheckPackageManifest(Action<bool> callback)
        {
            if (RuntimeMode != RuntimeMode.PackageOnly)
            {
                Debug.LogError("PackageOnly模式下才能调用CheckPackageManifest");
                callback(false);
                return;
            }

            string path = Util.GetReadOnlyPath(Util.ManifestFileName);

            WebRequestTask task = WebRequestTask.Create(loadTaskRunner, path, path, callback,
                (success, uwr, userdata) =>
                {
                    Action<bool> onChecked = (Action<bool>) userdata;

                    if (!success)
                    {
                        Debug.LogError($"单机模式资源清单检查失败:{uwr.error}");
                        onChecked?.Invoke(false);
                    }
                    else
                    {
                        CatAssetManifest manifest =
                            CatJson.JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

                        bundleRuntimeInfoDict.Clear();
                        assetRuntimeInfoDict.Clear();

                        foreach (BundleManifestInfo info in manifest.Bundles)
                        {
                            InitRuntimeInfo(info, false);
                        }

                        Debug.Log("单机模式资源清单检查完毕");
                        onChecked?.Invoke(true);
                    }
                });

            loadTaskRunner.AddTask(task, TaskPriority.Height);
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 加载资源
        /// </summary>
        public static int LoadAsset(string assetName, object userdata, LoadAssetCallback<Object> callback,
            TaskPriority priority = TaskPriority.Middle)
        {
            return LoadAsset<Object>(assetName, userdata, callback, priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static int LoadAsset<T>(string assetName, object userdata, LoadAssetCallback<T> callback,
            TaskPriority priority = TaskPriority.Middle) where T : Object
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                T asset;
                try
                {
                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetName);
                }
                catch (Exception e)
                {
                    callback?.Invoke(false, null, userdata);
                    throw;
                }

                callback?.Invoke(true, asset, userdata);
                return default;
            }
#endif

            //检查资源是否已在本地准备好
            if (!CheckAssetReady(assetName))
            {
                callback?.Invoke(false, null, userdata);
                return default;
            }

            AssetRuntimeInfo info = assetRuntimeInfoDict[assetName];

            Type assetType = typeof(T);
            if (assetType != info.AssetManifest.Type && assetType != typeof(Object))
            {
                Debug.LogError(
                    $"资源加载类型错误，资源名:{info.AssetManifest.Name},资源类型:{info.AssetManifest.Type.Name},加载类型:{typeof(T).Name}");
                callback?.Invoke(false, null, userdata);
                return default;
            }

            //开始加载
            LoadAssetTask<T> task = LoadAssetTask<T>.Create(loadTaskRunner, assetName, userdata, callback);
            loadTaskRunner.AddTask(task, priority);
            
            return task.GUID;
        }

        /// <summary>
        /// 批量加载资源
        /// </summary>
        public static int BatchLoadAsset(List<string> assetNames, object userdata, BatchLoadAssetCallback callback, TaskPriority priority = TaskPriority.Middle)
        {
            if (assetNames == null || assetNames.Count == 0)
            {
                Debug.LogError("批量加载资源失败，资源名列表为空");
                callback?.Invoke(null,userdata);
                return default;
            }
            
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                List<object> assets = new List<object>();
                foreach (string assetName in assetNames)
                {
                    Object asset = null;
                    try
                    {
                        asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(Object));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    finally
                    {
                        assets.Add(asset);
                    }
                    callback?.Invoke(assets,userdata);
                }

                return default;
            }
#endif
            
            BatchLoadAssetTask task = BatchLoadAssetTask.Create(loadTaskRunner,$"{nameof(BatchLoadAsset)} - {TaskRunner.GUIDFactory + 1}",assetNames,userdata,callback);
            loadTaskRunner.AddTask(task,priority);
            return task.GUID;
        }
        
        /// <summary>
        /// 加载场景
        /// </summary>
        public static int LoadScene(string sceneName, object userdata, LoadSceneCallback callback,
            TaskPriority priority = TaskPriority.Middle)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                try
                {
                    SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).completed += (op) =>
                    {
                        callback?.Invoke(true, SceneManager.GetSceneByPath(sceneName), userdata);
                    };
                }
                catch (Exception e)
                {
                    callback?.Invoke(false, default, userdata);
                    throw;
                }

                return default;
            }
#endif
            if (!CheckAssetReady(sceneName))
            {
                callback?.Invoke(false, default, userdata);
                return default;
            }

            //创建加载场景的任务
            LoadSceneTask task = LoadSceneTask.Create(loadTaskRunner, sceneName, userdata, callback);
            loadTaskRunner.AddTask(task, priority);

            return task.GUID;
        }

        /// <summary>
        /// 加载原生资源
        /// </summary>
        public static int LoadRawAsset(string assetName, object userdata,LoadRawAssetCallback callback,TaskPriority priority = TaskPriority.Middle)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                byte[] asset;
                try
                {
                    asset = File.ReadAllBytes(assetName);
                }
                catch (Exception e)
                {
                    callback?.Invoke(false, null, userdata);
                    throw;
                }

                callback?.Invoke(true, asset, userdata);
                return default;
            }
#endif
            //检查资源是否已在本地准备好
            if (!CheckAssetReady(assetName))
            {
                callback?.Invoke(false, null, userdata);
                return default;
            }

            LoadRawAssetTask task = LoadRawAssetTask.Create(loadTaskRunner,assetName,userdata,callback);
            loadTaskRunner.AddTask(task, priority);
            
            return task.GUID;
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public static void CancelTask(int guid)
        {
            if (allTaskDict.TryGetValue(guid,out ITask task))
            {
                task.Cancel();
            }
        }
        
        #endregion

        #region 资源卸载

        /// <summary>
        /// 卸载资源
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

            if (!assetInstanceDict.TryGetValue(asset, out AssetRuntimeInfo assetRuntimeInfo))
            {
                if (asset is Object unityObj)
                {
                    Debug.LogError($"要卸载的资源未加载过：{unityObj.name}");
                }
                else
                {
                    Debug.LogError("要卸载的资源未加载过");
                }

                return;
            }
            

            InternalUnloadAsset(assetRuntimeInfo);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public static void UnloadScene(Scene scene)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                SceneManager.UnloadSceneAsync(scene);
                return;
            }
#endif
            if (scene == default)
            {
                return;
            }

            if (!sceneInstanceDict.TryGetValue(scene.handle, out AssetRuntimeInfo assetRuntimeInfo))
            {
                Debug.LogError($"要卸载的场景未加载过：{scene.path}");
                return;
            }

            //卸载场景
            RemoveSceneInstance(scene);
            SceneManager.UnloadSceneAsync(scene);

            InternalUnloadAsset(assetRuntimeInfo);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        private static void InternalUnloadAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            assetRuntimeInfo.SubRefCount();

            //引用计数为0 卸载依赖
            if (assetRuntimeInfo.IsUnused())
            {
                if (assetRuntimeInfo.AssetManifest.Dependencies != null)
                {
                    foreach (string dependency in assetRuntimeInfo.AssetManifest.Dependencies)
                    {
                        AssetRuntimeInfo dependencyRuntimeInfo = GetAssetRuntimeInfo(dependency);
                        UnloadAsset(dependencyRuntimeInfo.Asset);
                    }
                }
            }
            
        }

        /// <summary>
        /// 卸载资源包
        /// </summary>
        internal static void UnloadBundle(BundleRuntimeInfo bundleRuntimeInfo)
        {
            UnloadBundleTask task = UnloadBundleTask.Create(loadTaskRunner,
                bundleRuntimeInfo.Manifest.RelativePath, bundleRuntimeInfo);
            loadTaskRunner.AddTask(task, TaskPriority.Low);
        }

        /// <summary>
        /// 卸载原生资源
        /// </summary>
        internal static void UnloadRawAsset(BundleRuntimeInfo bundleRuntimeInfo, AssetRuntimeInfo assetRuntimeInfo)
        {
            UnloadRawAssetTask task = UnloadRawAssetTask.Create(loadTaskRunner,bundleRuntimeInfo.Manifest.RelativePath,assetRuntimeInfo);
            loadTaskRunner.AddTask(task, TaskPriority.Low);
        }

        #endregion
    }
}