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
    public static partial class CatAssetManager
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
                        
                        CatAssetDatabase.InitManifest(manifest);
                        
                        Debug.Log("单机模式资源清单检查完毕");
                        onChecked?.Invoke(true);
                    }
                });

            loadTaskRunner.AddTask(task, TaskPriority.VeryHeight);
        }

        /// <summary>
        /// 检查资源版本，可更新资源模式下专用
        /// </summary>
        public static void CheckVersion(OnVersionChecked onVersionChecked)
        {
            if (RuntimeMode != RuntimeMode.Updatable)
            {
                Debug.LogError("非Updatable模式下不能调用CheckVersion");
                return;
            }
            
            VersionChecker.CheckVersion(onVersionChecked);
        }
        
        /// <summary>
        /// 检查可更新模式下指定路径的资源清单
        /// </summary>
        internal static void CheckUpdatableManifest(string path,WebRequestCallback callback)
        {
            WebRequestTask task = WebRequestTask.Create(downloadTaskRunner,path,path,null,callback);
            downloadTaskRunner.AddTask(task,TaskPriority.VeryHeight);
        }
        
        #endregion

        #region 资源加载
        
        /// <summary>
        /// <para>加载资源</para>
        /// <para>资源类别判断规则：</para>
        /// <para>1.内置Unity资源的assetName以"Assets/"开头</para>
        /// <para>2.内置原生资源的assetName以"raw:Assets/"开头</para>
        /// <para>3.否则视为外置原生资源</para>
        /// </summary>
        public static int LoadAsset(string assetName, object userdata, LoadAssetCallback callback,
            TaskPriority priority = TaskPriority.Middle)
        {
            AssetCategory category = Util.GetAssetCategory(assetName);
            if (category == AssetCategory.InternalRawAsset)
            {
                //内置原生资源需要去掉"raw:"
                assetName = Util.GetRealInternalRawAssetName(assetName);
            }
            return InternalLoadAsset(assetName, userdata, category, callback, priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        internal static int InternalLoadAsset(string assetName, object userdata,AssetCategory category, LoadAssetCallback callback,
            TaskPriority priority = TaskPriority.Middle)
        {

#if UNITY_EDITOR
            if (IsEditorMode)
            {
                object asset;

                try
                {
                    if (category == AssetCategory.InternalUnityAsset)
                    {
                        //加载Unity资源
                        asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetName);
                    }
                    else
                    {   
                        //加载原生资源
                        if (category == AssetCategory.ExternalRawAsset)
                        {
                            //编辑器资源模式下 加载外置原生资源 需要给出带读写区路径的完整assetName
                            assetName = Util.GetReadWritePath(assetName);
                        }
                    
                        asset = File.ReadAllBytes(assetName);
                    }
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

            switch (category)
            {

                case AssetCategory.InternalUnityAsset:
                    
                    //加载Unity资源
                    if (!CheckAssetReady(assetName))
                    {
                        callback?.Invoke(false, null, userdata);
                        return default;
                    }
                    LoadUnityAssetTask loadUnityAssetTask = LoadUnityAssetTask.Create(loadTaskRunner, assetName, userdata, callback);
                    loadTaskRunner.AddTask(loadUnityAssetTask, priority);
                    return loadUnityAssetTask.GUID;
                
                case AssetCategory.InternalRawAsset:
                    
                    //加载内置原生资源
                    if (!CheckAssetReady(assetName))
                    {
                        callback?.Invoke(false, null, userdata);
                        return default;
                    }
                    LoadRawAssetTask loadRawAssetTask = LoadRawAssetTask.Create(loadTaskRunner,assetName,userdata,callback);
                    loadTaskRunner.AddTask(loadRawAssetTask, priority);
            
                    return loadRawAssetTask.GUID;

                case AssetCategory.ExternalRawAsset:
                    
                    //加载外置原生资源
                    CatAssetDatabase.GetOrAddAssetRuntimeInfo(assetName);
                    loadRawAssetTask = LoadRawAssetTask.Create(loadTaskRunner,assetName,userdata,callback);
                    loadTaskRunner.AddTask(loadRawAssetTask, priority);
            
                    return loadRawAssetTask.GUID;
            }
            
            return default;

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
                    LoadAsset(assetName, null, ((success, asset, o) =>
                    {
                        assets.Add(asset);
                        if (assets.Count == assetNames.Count)
                        {
                            //编辑器模式下是以同步的方式加载所有资源的 所以这里的asset顺序是和assetNames给出的顺序可以对上的
                            callback(assets, userdata);
                        }
                    }));
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
            
            LoadSceneTask task = LoadSceneTask.Create(loadTaskRunner, sceneName, userdata, callback);
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

            AssetRuntimeInfo info = CatAssetDatabase.GetAssetRuntimeInfo(asset);
            
            if (info == null)
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
            

            InternalUnloadAsset(info);
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
            List<AssetRuntimeInfo> assets = CatAssetDatabase.GetSceneBindAssets(scene);
            if (assets != null)
            {
                foreach (AssetRuntimeInfo asset in assets)
                {
                    UnloadAsset(asset.Asset);
                }
            }

            InternalUnloadAsset(info);
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
                if (assetRuntimeInfo.AssetManifest.Dependencies != null)
                {
                    foreach (string dependency in assetRuntimeInfo.AssetManifest.Dependencies)
                    {
                        AssetRuntimeInfo dependencyRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependency);
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

        #region 资源生命周期绑定

        /// <summary>
        /// 将资源绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public static void BindToGameObject(GameObject target,Object asset)
        {
            AssetBinder assetBinder = target.GetOrAddComponent<AssetBinder>();
            assetBinder.BindTo(asset);
        }
        
        /// <summary>
        /// 将原生资源绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的原生资源
        /// </summary>
        public static void BindToGameObject(GameObject target,byte[] rawAsset)
        {
            AssetBinder assetBinder = target.GetOrAddComponent<AssetBinder>();
            assetBinder.BindTo(rawAsset);
        }

        /// <summary>
        /// 将资源绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public static void BindToScene(Scene scene,object asset)
        {
            CatAssetDatabase.AddSceneBindAsset(scene,asset);
        }
        
        #endregion
    }
}