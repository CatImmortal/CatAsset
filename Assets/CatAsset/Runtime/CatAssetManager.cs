using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        /// 卸载相关任务运行器
        /// </summary>
        private static TaskRunner unloadTaskRunner = new TaskRunner();
        
        /// <summary>
        /// 下载相关任务运行器
        /// </summary>
        private static TaskRunner downloadTaskRunner = new TaskRunner();
        
        /// <summary>
        /// 资源类型->自定义原生资源转换方法
        /// </summary>
        private static Dictionary<Type, CustomRawAssetConverter> converterDict =
            new Dictionary<Type, CustomRawAssetConverter>();

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
        public static float UnloadBundleDelayTime { get; set; }

        /// <summary>
        /// 资源卸载延迟时间
        /// </summary>
        public static float UnloadAssetDelayTime { get; set; }
        
        /// <summary>
        /// 单帧最大任务运行数量
        /// </summary>
        public static int MaxTaskRunCount
        {
            set
            {
                loadTaskRunner.MaxRunCount = value;
                downloadTaskRunner.MaxRunCount = value;
            }
        }

        /// <summary>
        /// 设置资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/BundleRelativePath 为下载地址
        /// </summary>
        public static string UpdateUriPrefix
        {
            get => CatAssetUpdater.UpdateUriPrefix;
            set => CatAssetUpdater.UpdateUriPrefix = value;
        }

        static CatAssetManager()
        {
            RegisterCustomRawAssetConverter(typeof(Texture2D), (bytes =>
            {
                Texture2D texture2D = new Texture2D(0, 0);
                texture2D.LoadImage(bytes);
                return texture2D;
            }));

            RegisterCustomRawAssetConverter(typeof(Sprite), (bytes =>
            {
                Texture2D texture2D = new Texture2D(0, 0);
                texture2D.LoadImage(bytes);
                Sprite sp = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
                return sp;
            }));

            RegisterCustomRawAssetConverter(typeof(TextAsset), (bytes =>
            {
                string text = Encoding.UTF8.GetString(bytes);
                TextAsset textAsset = new TextAsset(text);
                return textAsset;
            }));
        }

        /// <summary>
        /// 轮询CatAsset资源管理器
        /// </summary>
        public static void Update()
        {
            loadTaskRunner.Update();
            unloadTaskRunner.Update();
            downloadTaskRunner.Update();
        }
        
        /// <summary>
        /// 注册自定义原生资源转换方法
        /// </summary>
        public static void RegisterCustomRawAssetConverter(Type type, CustomRawAssetConverter converter)
        {
            converterDict[type] = converter;
        }

        /// <summary>
        /// 获取自定义原生资源转换方法
        /// </summary>
        internal static CustomRawAssetConverter GetCustomRawAssetConverter(Type type)
        {
            converterDict.TryGetValue(type, out CustomRawAssetConverter converter);
            return converter;
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
                        CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(uwr.downloadHandler.text);
                        CatAssetDatabase.InitPackageManifest(manifest);

                        Debug.Log("单机模式资源清单检查完毕");
                        onChecked?.Invoke(true);
                    }
                });

            loadTaskRunner.AddTask(task, TaskPriority.VeryLow);
        }

        /// <summary>
        /// 检查资源版本，可更新资源模式下专用
        /// </summary>
        public static void CheckVersion(OnVersionChecked onVersionChecked)
        {
            if (RuntimeMode != RuntimeMode.Updatable)
            {
                Debug.LogError("Updatable模式下才能调用CheckVersion");
                return;
            }

            VersionChecker.CheckVersion(onVersionChecked);
        }

        /// <summary>
        /// 检查可更新模式下指定路径的资源清单
        /// </summary>
        internal static void CheckUpdatableManifest(string path, WebRequestCallback callback)
        {
            WebRequestTask task = WebRequestTask.Create(downloadTaskRunner, path, path, null, callback);
            downloadTaskRunner.AddTask(task, TaskPriority.VeryLow);
        }

        /// <summary>
        /// 从外部导入内置资源
        /// </summary>
        public static void ImportInternalAsset(string manifestPath, Action<bool> callback,
            string bundleRelativePathPrefix = null)
        {
            manifestPath = Util.GetReadWritePath(manifestPath);
            WebRequestTask task = WebRequestTask.Create(loadTaskRunner, manifestPath, manifestPath, callback,
                (success, uwr, userdata) =>
                {
                    Action<bool> onChecked = (Action<bool>) userdata;

                    if (!success)
                    {
                        Debug.LogError($"内置资源导入失败:{uwr.error}");
                        onChecked?.Invoke(false);
                    }
                    else
                    {
                        CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(uwr.downloadHandler.text);

                        foreach (BundleManifestInfo bundleManifestInfo in manifest.Bundles)
                        {
                            if (!string.IsNullOrEmpty(bundleRelativePathPrefix))
                            {
                                //为资源包目录名添加额外前缀
                                bundleManifestInfo.Directory = Util.GetRegularPath(Path.Combine(bundleRelativePathPrefix,
                                    bundleManifestInfo.Directory));
                            }

                            CatAssetDatabase.InitRuntimeInfo(bundleManifestInfo, true);
                        }

                        Debug.Log("内置资源导入完毕");
                        onChecked?.Invoke(true);
                    }
                });

            loadTaskRunner.AddTask(task, TaskPriority.Height);
        }

        #endregion

        #region 资源更新

        /// <summary>
        /// 更新资源组
        /// </summary>
        public static void UpdateGroup(string group, OnBundleUpdated callback)
        {
            CatAssetUpdater.UpdateGroup(group, callback);
        }

        /// <summary>
        /// 暂停资源组更新
        /// </summary>
        public static void PauseGroupUpdater(string group, bool isPause)
        {
            CatAssetUpdater.PauseGroupUpdate(group, isPause);
        }

        /// <summary>
        /// 下载资源包
        /// </summary>
        internal static void DownloadBundle(GroupUpdater groupUpdater, BundleManifestInfo info, string downloadUri,
            string localFilePath, DownloadBundleCallback callback,TaskPriority priority = TaskPriority.Middle)
        {
            DownloadBundleTask task =
                DownloadBundleTask.Create(downloadTaskRunner, downloadUri, info, groupUpdater, downloadUri,
                    localFilePath, callback);
            downloadTaskRunner.AddTask(task, priority);
        }

        #endregion

        #region 资源加载

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
            }));
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
            }));
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
            }));
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

        /// <summary>
        /// 取消任务
        /// </summary>
        public static void CancelTask(int taskID)
        {
            if (TaskRunner.TaskIDDict.TryGetValue(taskID, out ITask task))
            {
                task.Cancel();
            }
        }
        
        /// <summary>
        /// 获取任务进度
        /// </summary>
        public static float GetTaskProgress(int taskID)
        {
            if (TaskRunner.TaskIDDict.TryGetValue(taskID, out ITask task))
            {
                return task.Progress;
            }

            return -1;
        }

        #endregion

        #region 资源卸载

        /// <summary>
        /// 卸载资源，注意：asset参数需要是原始资源实例,可以从LoadAssetResult.Asset获取
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

        /// <summary>
        /// 卸载场景
        /// </summary>
        public static void UnloadScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }
            
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                SceneManager.UnloadSceneAsync(scene);
                return;
            }
#endif
            AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(scene);

            if (assetRuntimeInfo == null)
            {
                Debug.LogError($"要卸载的场景未加载过：{scene.path}");
                return;
            }

            //卸载场景
            CatAssetDatabase.RemoveSceneInstance(scene);
            SceneManager.UnloadSceneAsync(scene);

            //卸载与场景绑定的资源
            List<AssetRuntimeInfo> bindAssets = CatAssetDatabase.GetSceneBindAssets(scene);
            if (bindAssets != null)
            {
                foreach (AssetRuntimeInfo bindAsset in bindAssets)
                {
                    InternalUnloadAsset(bindAsset);
                }
            }

            InternalUnloadAsset(assetRuntimeInfo);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        internal static void InternalUnloadAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            //减少引用计数
            assetRuntimeInfo.SubRefCount();

            if (assetRuntimeInfo.IsUnused())
            {
                //引用计数为0
                //卸载依赖
                foreach (string dependency in assetRuntimeInfo.AssetManifest.Dependencies)
                {
                    AssetRuntimeInfo dependencyRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependency);
                    InternalUnloadAsset(dependencyRuntimeInfo);
                    
                    //删除依赖链记录
                    dependencyRuntimeInfo.RemoveDownStream(assetRuntimeInfo);
                }

                //对于非场景 非Prefab的资源 以及原生资源 创建卸载任务
                BundleRuntimeInfo bundleRuntimeInfo =
                    CatAssetDatabase.GetBundleRuntimeInfo(assetRuntimeInfo.BundleManifest.RelativePath);
                if (!bundleRuntimeInfo.Manifest.IsScene && !(assetRuntimeInfo.Asset is GameObject))
                {
                    UnloadAssetTask task = UnloadAssetTask.Create(unloadTaskRunner,assetRuntimeInfo.AssetManifest.Name,assetRuntimeInfo);
                    unloadTaskRunner.AddTask(task,TaskPriority.Low);
                }
            }
        }

        /// <summary>
        /// 卸载所有未使用的资源，若isQuick为true则是不处理Prefab的快速模式
        /// </summary>
        public static void UnloadUnusedAssets(bool isQuickMode = true)
        {
            foreach (KeyValuePair<string,BundleRuntimeInfo> pair in CatAssetDatabase.GetAllBundleRuntimeInfo())
            {
                BundleRuntimeInfo bundleRuntimeInfo = pair.Value;

                if (!bundleRuntimeInfo.Manifest.IsRaw && bundleRuntimeInfo.Bundle == null)
                {
                    //跳过未加载的资源包
                    continue;
                }

                if (bundleRuntimeInfo.Manifest.IsScene)
                {
                    //跳过场景资源包
                    continue;
                }
                
                foreach (AssetManifestInfo assetManifestInfo in bundleRuntimeInfo.Manifest.Assets)
                {
                    AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetManifestInfo.Name);

                    if (assetRuntimeInfo.Asset == null || assetRuntimeInfo.RefCount > 0)
                    {
                        //资源未加载 或 引用计数>0 跳过
                        continue;
                    }

                    if (!isQuickMode)
                    {
                        //非快速模式下 只解除引用 不调用Resources.UnloadAsset 等待后面的Resources.UnloadUnusedAssets来卸载
                        CatAssetDatabase.RemoveAssetInstance(assetRuntimeInfo.Asset);
                        assetRuntimeInfo.Asset = null;
                        continue;
                    }
                  
                    //快速模式下 只处理非Prefab资源
                    if (!(assetRuntimeInfo.Asset is GameObject))
                    {
                        CatAssetDatabase.RemoveAssetInstance(assetRuntimeInfo.Asset);
                        if (assetRuntimeInfo.Asset is Object unityObj)
                        {
                            UnloadAssetFromMemory(unityObj);
                        }
                        assetRuntimeInfo.Asset = null;
                    }

                    
                }
            }

            if (!isQuickMode)
            {
                Resources.UnloadUnusedAssets();
            }
            
            Debug.Log("已卸载所有未使用的资源");
        }

        /// <summary>
        /// 将资源包资源从内存卸载
        /// </summary>
        internal static void UnloadAssetFromMemory(Object asset)
        {
            if (asset is Sprite sprite)
            {
                //注意 sprite资源得卸载它的texture
                Resources.UnloadAsset(sprite.texture);
            }
            else
            {
                Resources.UnloadAsset(asset);
            }
        }
        
        /// <summary>
        /// 添加卸载资源包任务
        /// </summary>
        internal static void AddUnloadBundleTask(BundleRuntimeInfo bundleRuntimeInfo)
        {
            UnloadBundleTask task = UnloadBundleTask.Create(unloadTaskRunner,
                bundleRuntimeInfo.Manifest.RelativePath, bundleRuntimeInfo);
            unloadTaskRunner.AddTask(task, TaskPriority.Low);
        }
        

        #endregion

        #region 资源生命周期绑定

        /// <summary>
        /// 将资源绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public static void BindToGameObject(GameObject target, object asset)
        {
            if (asset == null)
            {
                return;
            }
            
            AssetBinder assetBinder = target.GetOrAddComponent<AssetBinder>();
            assetBinder.BindTo(asset);
        }

        /// <summary>
        /// 将资源绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public static void BindToScene(Scene scene, object asset)
        {
            if (asset == null)
            {
                return;
            }

            if (!scene.isLoaded)
            {
                return;
            }
            
            CatAssetDatabase.AddSceneBindAsset(scene, asset);
        }

        #endregion

        #region 数据获取代理

        /// <summary>
        /// 获取资源组信息
        /// </summary>
        public static GroupInfo GetGroupInfo(string group)
        {
            return CatAssetDatabase.GetGroupInfo(group);
        }

        /// <summary>
        /// 获取所有资源组信息
        /// </summary>
        public static List<GroupInfo> GetAllGroupInfo()
        {
            return CatAssetDatabase.GetAllGroupInfo();
        }

        /// <summary>
        /// 获取指定资源组的更新器
        /// </summary>
        public static GroupUpdater GetGroupUpdater(string group)
        {
            return CatAssetUpdater.GetGroupUpdater(group);
        }

        #endregion
    }
}