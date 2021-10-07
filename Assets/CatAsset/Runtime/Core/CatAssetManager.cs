using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
namespace CatAsset
{
    /// <summary>
    /// CatAsset资源管理器
    /// </summary>
    public class CatAssetManager
    {
        /// <summary>
        /// AssetBundle运行时信息字典
        /// </summary>
        private static Dictionary<string, AssetBundleRuntimeInfo> assetBundleInfoDict = new Dictionary<string, AssetBundleRuntimeInfo>();

        /// <summary>
        /// Asset运行时信息字典
        /// </summary>
        private static Dictionary<string, AssetRuntimeInfo> assetInfoDict = new Dictionary<string, AssetRuntimeInfo>();

        /// <summary>
        /// 远端Asset名与对应AssetBundle清单信息的映射字典
        /// </summary>
        private static Dictionary<string, AssetBundleManifestInfo> remoteAssetToAssetBundleDict = new Dictionary<string, AssetBundleManifestInfo>();

        /// <summary>
        /// Asset和Asset运行时信息的映射字典(不包括场景)
        /// </summary>
        private static Dictionary<Object, AssetRuntimeInfo> assetToAssetInfo = new Dictionary<Object, AssetRuntimeInfo>();

        /// <summary>
        /// 资源组信息字典
        /// </summary>
        internal static Dictionary<string, GroupInfo> groupInfoDict = new Dictionary<string, GroupInfo>();

        /// <summary>
        /// 任务执行器
        /// </summary>
        internal static TaskExcutor taskExcutor = new TaskExcutor();

        /// <summary>
        /// 编辑器资源模式下的最大加载延时
        /// </summary>
        internal static float EditorModeMaxDelay;

        /// <summary>
        /// 资源卸载延迟时间
        /// </summary>
        internal static float UnloadDelayTime;

        /// <summary>
        /// 单帧最大任务执行数量
        /// </summary>
        internal static int MaxTaskExcuteCount
        {
            set
            {
                taskExcutor.MaxExcuteCount = value;
            }
        }

        /// <summary>
        /// 运行模式
        /// </summary>
        public static RunMode RunMode
        {
            get;
            internal set;
        }

        /// <summary>
        /// 是否开启编辑器资源模式
        /// </summary>
        public static bool IsEditorMode
        {
            get;
            internal set;
        }

        /// <summary>
        /// 资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/AssetBundleName 为下载地址
        /// </summary>
        public static string UpdateUriPrefix
        {
            get
            {
                return CatAssetUpdater.UpdateUriPrefix;
            }

            set
            {
                CatAssetUpdater.UpdateUriPrefix = value;
            }
        }

        /// <summary>
        /// 添加资源运行时信息
        /// </summary>
        internal static void AddRuntimeInfo(AssetBundleManifestInfo abInfo, bool inReadWrite)
        {
            if (!assetBundleInfoDict.TryGetValue(abInfo.AssetBundleName, out AssetBundleRuntimeInfo abRuntimeInfo))
            {
                abRuntimeInfo = new AssetBundleRuntimeInfo();
                assetBundleInfoDict.Add(abInfo.AssetBundleName, abRuntimeInfo);
            }

            abRuntimeInfo.ManifestInfo = abInfo;
            abRuntimeInfo.InReadWrite = inReadWrite;

            foreach (AssetManifestInfo assetManifestInfo in abInfo.Assets)
            {
                if (!assetInfoDict.TryGetValue(assetManifestInfo.AssetName, out AssetRuntimeInfo assetRuntimeInfo))
                {
                    assetRuntimeInfo = new AssetRuntimeInfo();
                    assetInfoDict.Add(assetManifestInfo.AssetName, assetRuntimeInfo);
                }

                assetRuntimeInfo.ManifestInfo = assetManifestInfo;
                assetRuntimeInfo.AssetBundleName = abInfo.AssetBundleName;
            }
        }

        /// <summary>
        /// 获取AssetBundle运行时信息
        /// </summary>
        internal static AssetBundleRuntimeInfo GetAssetBundleRuntimeInfo(string assetBundleName)
        {
            return assetBundleInfoDict[assetBundleName];
        }

        /// <summary>
        /// 获取Asset运行时信息
        /// </summary>
        internal static AssetRuntimeInfo GetAssetRuntimeInfo(string assetName)
        {
            return assetInfoDict[assetName];
        }

        /// <summary>
        /// 添加远端Asset清单信息
        /// </summary>
        internal static void AddRemoteAssetManifestInfo(CatAssetManifest remoteManifest)
        {
            remoteAssetToAssetBundleDict.Clear();
            foreach (AssetBundleManifestInfo abInfo in remoteManifest.AssetBundles)
            {
                foreach (AssetManifestInfo assetInfo in abInfo.Assets)
                {
                    remoteAssetToAssetBundleDict.Add(assetInfo.AssetName, abInfo);
                }
            }
        }

        /// <summary>
        /// 添加Asset到Asset运行时信息的映射
        /// </summary>
        internal static void AddAssetToRuntimeInfo(Object asset, AssetRuntimeInfo info)
        {
            assetToAssetInfo.Add(asset, info);
        }

        /// <summary>
        /// 移除Asset到Asset运行时信息的映射
        /// </summary>
        internal static void RemoveAssetToRuntimeInfo(Object asset)
        {
            assetToAssetInfo.Remove(asset);
        }

        /// <summary>
        /// 轮询CatAsset管理器
        /// </summary>
        public static void Update()
        {
            taskExcutor.Update();
        }

        /// <summary>
        /// 获取资源组信息
        /// </summary>
        public static GroupInfo GetGroupInfo(string group)
        {
            if (!groupInfoDict.TryGetValue(group,out GroupInfo groupInfo))
            {
                groupInfo = new GroupInfo();
                groupInfo.GroupName = group;
                groupInfoDict.Add(group, groupInfo);
            }

            return groupInfo;
        }

        /// <summary>
        /// 检查安装包内资源清单,仅使用安装包内资源模式下专用
        /// </summary>
        public static void CheckPackageManifest(Action<bool> callback)
        {
            if (RunMode != RunMode.PackageOnly)
            {
                Debug.LogError("PackageOnly模式下才能调用CheckPackageManifest");
                return;
            }

            string path = Util.GetReadOnlyPath(Util.ManifestFileName);

            WebRequestTask task = new WebRequestTask(taskExcutor, path,path, (success, error, uwr) => {
                if (!success)
                {
                    Debug.LogError("单机模式资源清单检查失败");
                    callback?.Invoke(false);
                    return;
                }
                CatAssetManifest manifest = CatJson.JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);
                foreach (AssetBundleManifestInfo abInfo in manifest.AssetBundles)
                {
                    AddRuntimeInfo(abInfo, false);
                }
                callback?.Invoke(true);
            });

            taskExcutor.AddTask(task);
        }

        /// <summary>
        /// 检查资源版本,可更新模式与边玩边下模式专用
        /// </summary>
        public static void CheckVersion(Action<int, long,string> onVersionChecked,string checkGroup = null)
        {
            CatAssetUpdater.CheckVersion(onVersionChecked,checkGroup);
        }

        /// <summary>
        /// 更新资源
        /// </summary>
        public static void UpdateAsset(Action<int, long, int, long, string, string> onFileDownloaded,string updateGroup = null)
        {
            CatAssetUpdater.UpdateAsset(onFileDownloaded,updateGroup);
        }

        /// <summary>
        /// 暂停更新资源
        /// </summary>
        public static void PauseUpdateAsset(string group = null)
        {
            CatAssetUpdater.PauseUpdater(true, group);
        }

        /// <summary>
        /// 恢复更新资源
        /// </summary>
        public static void ResumeUpdateAsset(string group = null)
        {
            CatAssetUpdater.PauseUpdater(false, group);
        }

        /// <summary>
        /// 加载Asset
        /// </summary>
        public static void LoadAsset(string assetName, Action<bool, Object> loadedCallback)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                EditorLoadAssetTask editorModeTask = new EditorLoadAssetTask(taskExcutor, assetName,loadedCallback);
                taskExcutor.AddTask(editorModeTask);
                return;
            }
#endif

            if (assetBundleInfoDict.Count == 0)
            {
                Debug.LogError("Asset加载失败,未进行资源清单检查");
                return;
            }

            if (!assetInfoDict.TryGetValue(assetName, out AssetRuntimeInfo assetInfo))
            {
                if (RunMode != RunMode.UpdatableWhilePlaying || !remoteAssetToAssetBundleDict.ContainsKey(assetName))
                {
                    Debug.LogError("Asset加载失败，不在资源清单中：" + assetName);
                    return;
                }

                //开启了边玩边下模式 并且此asset所属ab在远端存在 尝试从远端下载对应ab
                AssetBundleManifestInfo abInfo = remoteAssetToAssetBundleDict[assetName];
                Updater updater = new Updater();
                updater.UpdateList.Add(abInfo);
                updater.TotalCount = 1;
                updater.TotalLength += abInfo.Length;

                updater.UpdateAsset((count, length, totalCount, totalLength, abName, group) => {
                    LoadAsset(assetName, loadedCallback);
                });
                return;
            }

            //加载依赖的Asset
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependency = assetInfo.ManifestInfo.Dependencies[i];
                LoadAsset(dependency, null);
            }


            if (assetInfo.UseCount == 0)
            {
                //标记进所属的AssetBundle的 使用中Asset集合 
                AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
                abInfo.UsedAssets.Add(assetInfo.ManifestInfo.AssetName);
            }

            //增加引用计数
            assetInfo.UseCount++;

            if (assetInfo.Asset != null)
            {
                //已加载过 直接调用回调方法
                loadedCallback?.Invoke(true,assetInfo.Asset);
                return;
            }

            //未加载 创建加载Asset的任务
            LoadAssetTask task = new LoadAssetTask(taskExcutor, assetName, loadedCallback);
            taskExcutor.AddTask(task);
        }

        /// <summary>
        /// 卸载Asset
        /// </summary>
        public static void UnloadAsset(Object asset)
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

            if (!assetToAssetInfo.TryGetValue(asset, out AssetRuntimeInfo assetInfo))
            {
                Debug.LogError("要卸载的Asset未加载过：" + asset.name);
                return;
            }

            if (assetInfo.UseCount == 0)
            {
                return;
            }

            //卸载依赖资源
            foreach (string dependency in assetInfo.ManifestInfo.Dependencies)
            {
                AssetRuntimeInfo dependencyInfo = assetInfoDict[dependency];
                UnloadAsset(dependencyInfo.Asset);
            }

            //减少Asset的引用计数
            assetInfo.UseCount--;

            if (assetInfo.UseCount == 0)
            {
                //Asset已经没人使用了
                //从所属的AssetBundle的 UsedAsset 中移除
                AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
                abInfo.UsedAssets.Remove(assetInfo.ManifestInfo.AssetName);

                if (abInfo.UsedAssets.Count == 0)
                {
                    //AssetBundle也已经没人使用了 创建卸载任务 开始卸载倒计时
                    UnloadAssetBundleTask task = new UnloadAssetBundleTask(taskExcutor, abInfo.ManifestInfo.AssetBundleName);
                    taskExcutor.AddTask(task);
                    Debug.Log("创建了卸载AB的任务：" + task.Name);
                }
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public static void LoadScene(string sceneName, Action<bool, Object> loadedCallback)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);
                asyncOp.completed += (op) =>
                {
                    if (op.isDone)
                    {
                        loadedCallback?.Invoke(true,null);
                    }
                };
                return;
            }
#endif

            if (assetBundleInfoDict.Count == 0)
            {
                Debug.LogError("场景加载失败,未进行资源清单检查");
                return;
            }

            if (!assetInfoDict.TryGetValue(sceneName, out AssetRuntimeInfo assetInfo))
            {
                Debug.LogError("场景加载失败，该场景不在资源清单中：" + sceneName);
                return;
            }

            //加载依赖的Asset
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependency = assetInfo.ManifestInfo.Dependencies[i];
                LoadAsset(dependency, null);
            }

            if (assetInfo.UseCount == 0)
            {
                //标记进所属的AssetBundle的 使用中Asset集合
                AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
                abInfo.UsedAssets.Add(assetInfo.ManifestInfo.AssetName);
            }

            //增加引用计数
            assetInfo.UseCount++;

            //场景资源实例不能被复用 每次加载都得创建加载场景的任务
            LoadSceneTask task = new LoadSceneTask(taskExcutor, sceneName,loadedCallback);
            taskExcutor.AddTask(task);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public static void UnloadScene(string sceneName)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                SceneManager.UnloadSceneAsync(sceneName);
                return;
            }
#endif

            if (!assetInfoDict.TryGetValue(sceneName, out AssetRuntimeInfo assetInfo))
            {
                Debug.LogError("要卸载的Scene不在资源清单中：" + sceneName);
                return;
            }

            if (assetInfo.UseCount == 0)
            {
                return;
            }

            //卸载依赖资源
            foreach (string dependency in assetInfo.ManifestInfo.Dependencies)
            {
                AssetRuntimeInfo dependencyInfo = assetInfoDict[dependency];
                UnloadAsset(dependencyInfo.Asset);
            }

            //卸载场景
            SceneManager.UnloadSceneAsync(sceneName);

            //减少场景的引用计数
            assetInfo.UseCount--;
            if (assetInfo.UseCount == 0)
            {
                //场景已经没人使用了
                //从所属的AssetBundle的 UsedAsset 中移除
                AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
                abInfo.UsedAssets.Remove(assetInfo.ManifestInfo.AssetName);

                if (abInfo.UsedAssets.Count == 0)
                {
                    //AssetBundel也已经没人使用了 创建卸载任务 开始卸载倒计时
                    UnloadAssetBundleTask task = new UnloadAssetBundleTask(taskExcutor, abInfo.ManifestInfo.AssetBundleName);
                    taskExcutor.AddTask(task);
                    Debug.Log("创建了卸载AB的任务：" + task.Name);
                }
            }

        }

        /// <summary>
        /// 批量加载Asset
        /// </summary>
        public static void LoadAssets(List<string> assetNames, Action<List<Object>> loadedCallback)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                EditorLoadAssetsTask editorModeTask = new EditorLoadAssetsTask(taskExcutor, nameof(EditorLoadAssetsTask), assetNames,loadedCallback);
                taskExcutor.AddTask(editorModeTask);
                return;
            }
#endif

            if (assetBundleInfoDict.Count == 0)
            {
                Debug.LogError("Asset加载失败,未进行资源清单检查");
                return;
            }

            //创建批量加载Asset的任务
            LoadAssetsTask task = new LoadAssetsTask(taskExcutor, nameof(LoadAssetsTask), assetNames, loadedCallback);
            taskExcutor.AddTask(task);
        }


    }
}

