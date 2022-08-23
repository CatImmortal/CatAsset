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
        /// 场景实例handler->绑定的资源
        /// </summary>
        private static Dictionary<int, List<AssetRuntimeInfo>> sceneBindAssets =
            new Dictionary<int, List<AssetRuntimeInfo>>();

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
        /// 获取资源运行时信息，若不存在则添加（主要用于外置原生资源）
        /// </summary>
        private static AssetRuntimeInfo GetOrAddAssetRuntimeInfo(string assetName)
        {
            if (!assetRuntimeInfoDict.TryGetValue(assetName,out AssetRuntimeInfo assetRuntimeInfo))
            {
                int index = assetName.LastIndexOf('/');
                string dir = null;
                string name;
                if (index >= 0)
                {
                    dir = assetName.Substring(0, index - 1);
                    name = assetName.Substring(index + 1);
                }
                else
                {
                    name = assetName;
                }
                
                
                //创建外置原生资源的资源运行时信息
                assetRuntimeInfo = new AssetRuntimeInfo();
                assetRuntimeInfo.AssetManifest = new AssetManifestInfo
                {
                    Name = assetName,
                    Type = typeof(byte[])
                };
                assetRuntimeInfo.BundleManifest = new BundleManifestInfo
                {
                    RelativePath = assetName,
                    Directory = dir,
                    BundleName = name,
                    Group = string.Empty,
                    IsRaw = true,
                    IsScene = false,
                    Assets = new List<AssetManifestInfo>(){assetRuntimeInfo.AssetManifest},
                };
                assetRuntimeInfoDict.Add(assetName,assetRuntimeInfo);

                //创建外置原生资源的资源包运行时信息（是虚拟出的）
                BundleRuntimeInfo bundleRuntimeInfo = new BundleRuntimeInfo
                {
                    Manifest = assetRuntimeInfo.BundleManifest,
                    InReadWrite = true,
                };
                bundleRuntimeInfoDict.Add(bundleRuntimeInfo.Manifest.RelativePath,bundleRuntimeInfo);
            }

            return assetRuntimeInfo;
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
                    GetOrAddAssetRuntimeInfo(assetName);
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

            //卸载与场景绑定的资源
            if (sceneBindAssets.TryGetValue(scene.handle,out List<AssetRuntimeInfo> assets))
            {
                foreach (AssetRuntimeInfo asset in assets)
                {
                    UnloadAsset(asset.Asset);
                }
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
            if (!sceneBindAssets.TryGetValue(scene.handle,out List<AssetRuntimeInfo> assets))
            {
                assets = new List<AssetRuntimeInfo>();
                sceneBindAssets.Add(scene.handle,assets);
            }

            AssetRuntimeInfo assetRuntimeInfo = GetAssetRuntimeInfo(asset);
            assets.Add(assetRuntimeInfo);
        }

        
        
        #endregion
    }
}