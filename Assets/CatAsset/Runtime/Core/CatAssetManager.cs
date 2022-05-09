using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;

namespace CatAsset
{
    /// <summary>
    /// CatAsset资源管理器
    /// </summary>
    public class CatAssetManager
    {
        /// <summary>
        /// Bundle运行时信息字典（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        internal static Dictionary<string, BundleRuntimeInfo> bundleInfoDict = new Dictionary<string, BundleRuntimeInfo>();

        /// <summary>
        /// Asset运行时信息字典（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        internal static Dictionary<string, AssetRuntimeInfo> assetInfoDict = new Dictionary<string, AssetRuntimeInfo>();

        /// <summary>
        /// Asset和Asset运行时信息的映射字典(不包括场景)
        /// </summary>
        internal static Dictionary<Object, AssetRuntimeInfo> assetToAssetInfoDict = new Dictionary<Object, AssetRuntimeInfo>();

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
        /// 资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/BundleName 为下载地址
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
        /// 轮询CatAsset管理器
        /// </summary>
        public static void Update()
        {
            taskExcutor.Update();
        }

        #region 资源清单检查

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

            WebRequestTask task = new WebRequestTask(taskExcutor, path, path, (success, error, uwr) => {
                if (!success)
                {
                    Debug.LogError("单机模式资源清单检查失败");
                    callback?.Invoke(false);
                    return;
                }
                CatAssetManifest manifest = CatJson.JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);
                foreach (BundleManifestInfo abInfo in manifest.Bundles)
                {
                    InitRuntimeInfo(abInfo, false);
                }
                callback?.Invoke(true);
            });

            taskExcutor.AddTask(task);
        }

        /// <summary>
        /// 检查资源版本,可更新模式与边玩边下模式专用
        /// </summary>
        public static void CheckVersion(Action<int, long> onVersionChecked)
        {
            if (RunMode == RunMode.PackageOnly)
            {
                Debug.LogError("PackageOnly模式下不能调用CheckVersion");
                return;
            }

            CatAssetUpdater.CheckVersion(onVersionChecked);
        }


        /// <summary>
        /// 根据资源清单信息初始化运行时信息
        /// </summary>
        internal static void InitRuntimeInfo(BundleManifestInfo bundleManifestInfo, bool inReadWrite)
        {
            BundleRuntimeInfo bundleRuntimeInfo = new BundleRuntimeInfo();
            bundleInfoDict.Add(bundleManifestInfo.BundleName, bundleRuntimeInfo);
            bundleRuntimeInfo.ManifestInfo = bundleManifestInfo;
            bundleRuntimeInfo.InReadWrite = inReadWrite;

            foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
            {
                AssetRuntimeInfo assetRuntimeInfo = new AssetRuntimeInfo();
                assetInfoDict.Add(assetManifestInfo.AssetName, assetRuntimeInfo);
                assetRuntimeInfo.ManifestInfo = assetManifestInfo;
                assetRuntimeInfo.BundleName = bundleManifestInfo.BundleName;
            }
        }

        /// <summary>
        /// 获取资源组信息，若不存在则添加
        /// </summary>
        internal static GroupInfo GetOrCreateGroupInfo(string group)
        {
            if (!groupInfoDict.TryGetValue(group, out GroupInfo groupInfo))
            {
                groupInfo = new GroupInfo();
                groupInfo.GroupName = group;
                groupInfoDict.Add(group, groupInfo);
            }

            return groupInfo;
        }

        /// <summary>
        /// 获取指定资源组
        /// </summary>
        public static GroupInfo GetGroupInfo(string group)
        {
            if (!groupInfoDict.TryGetValue(group, out GroupInfo groupInfo))
            {
                Debug.LogError("不存在此资源组：" + group);
            }
            return groupInfo;
        }

        /// <summary>
        /// 获取所有资源组信息
        /// </summary>
        public static List<GroupInfo> GetAllGroup()
        {
            List<GroupInfo> result = new List<GroupInfo>();
            foreach (KeyValuePair<string, GroupInfo> item in groupInfoDict)
            {
                result.Add(item.Value);
            }
            return result;
        }

        #endregion

        #region 资源更新

        /// <summary>
        /// 更新资源
        /// </summary>
        public static void UpdateAssets(Action<bool, int, long, int, long, string, string> onUpdated, string updateGroup)
        {
            if (RunMode == RunMode.PackageOnly)
            {
                Debug.LogError("PackageOnly模式下不能调用UpdateAsset");
                return;
            }

            CatAssetUpdater.UpdateAssets(onUpdated, updateGroup);
        }

        /// <summary>
        /// 暂停更新资源
        /// </summary>
        public static void PauseUpdateAsset(string group)
        {
            CatAssetUpdater.PauseUpdater(true, group);
        }

        /// <summary>
        /// 恢复更新资源
        /// </summary>
        public static void ResumeUpdateAsset(string group)
        {
            CatAssetUpdater.PauseUpdater(false, group);
        }

        /// <summary>
        /// 获取指定组的更新器
        /// </summary>
        public static Updater GetUpdater(string group)
        {
            CatAssetUpdater.groupUpdaterDict.TryGetValue(group, out Updater result);
            return result;
        }

        /// <summary>
        /// 获取所有更新器
        /// </summary>
        /// <returns></returns>
        public static List<Updater> GetAllUpdater()
        {
            List<Updater> result = new List<Updater>(CatAssetUpdater.groupUpdaterDict.Values);
            return result;
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 加载Asset
        /// </summary>
        public static void LoadAsset(string assetName, Action<bool, Object> loadedCallback)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                EditorLoadAssetTask editorModeTask = new EditorLoadAssetTask(taskExcutor, assetName, loadedCallback);
                taskExcutor.AddTask(editorModeTask);
                return;
            }
#endif
            //检查Asset是否已在本地准备好
            if (!CheckAssetReady(assetName))
            {
                return;
            }

            //创建加载Asset的任务
            LoadAssetTask task = new LoadAssetTask(taskExcutor, assetName, loadedCallback);
            taskExcutor.AddTask(task);
        }


        /// <summary>
        /// 加载场景
        /// </summary>
        public static void LoadScene(string sceneName, Action<bool, Object> loadedCallback)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).completed += (op) =>
                {
                    loadedCallback?.Invoke(true,null);
                };
                return;
            }
#endif
            if (!CheckAssetReady(sceneName))
            {
                return;
            }
            //创建加载场景的任务
            LoadSceneTask task = new LoadSceneTask(taskExcutor, sceneName, loadedCallback);
            taskExcutor.AddTask(task);
        }

        /// <summary>
        /// 批量加载Asset
        /// </summary>
        public static void LoadAssets(List<string> assetNames, Action<List<Object>> loadedCallback)
        {
            if (assetNames == null || assetNames.Count == 0)
            {
                Debug.LogError("批量加载Asset失败，assetNames为空或数量为0");
                return;
            }

#if UNITY_EDITOR
            if (IsEditorMode)
            {
                EditorLoadAssetsTask editorModeTask = new EditorLoadAssetsTask(taskExcutor, nameof(EditorLoadAssetsTask), assetNames, loadedCallback);
                taskExcutor.AddTask(editorModeTask);
                return;
            }
#endif
            //创建批量加载Asset的任务
            LoadAssetsTask task = new LoadAssetsTask(taskExcutor, nameof(LoadAssetsTask) + Time.frameCount + UnityEngine.Random.Range(0f, 100f), assetNames, loadedCallback);
            taskExcutor.AddTask(task);
        }

        /// <summary>
        /// 检查Asset是否已准备好
        /// </summary>
        private static bool CheckAssetReady(string assetName)
        {
            if (!assetInfoDict.ContainsKey(assetName))
            {
                Debug.LogError("Asset加载失败，不在资源清单中：" + assetName);
                return false;
            }

            return true;
        }
        #endregion

        #region 资源卸载

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

            if (!assetToAssetInfoDict.TryGetValue(asset, out AssetRuntimeInfo assetInfo))
            {
                Debug.LogError("要卸载的Asset未加载过：" + asset.name);
                return;
            }

            InternalUnloadAsset(assetInfo);
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

            InternalUnloadAsset(assetInfo);
        }


        /// <summary>
        /// 卸载Asset
        /// </summary>
        private static void InternalUnloadAsset(AssetRuntimeInfo assetInfo)
        {

            if (assetInfo.RefCount == 0)
            {
                return;
            }

            //卸载依赖资源
            foreach (string dependency in assetInfo.ManifestInfo.Dependencies)
            {
                if (assetInfoDict.TryGetValue(dependency, out AssetRuntimeInfo dependencyInfo) && dependencyInfo.Asset != null)
                {
                    UnloadAsset(dependencyInfo.Asset);
                }
            }

            BundleRuntimeInfo bundleInfo = bundleInfoDict[assetInfo.BundleName];
            if (bundleInfo.ManifestInfo.IsScene)
            {
                //卸载场景
                SceneManager.UnloadSceneAsync(assetInfo.ManifestInfo.AssetName);
            }

            //减少引用计数
            assetInfo.RefCount--;

            if (assetInfo.RefCount == 0)
            {
                //已经没人在使用这个Asset了
                //从Bundle的 UsedAsset 中移除
                bundleInfo.UsedAssets.Remove(assetInfo.ManifestInfo.AssetName);
                CheckBundleLifeCycle(bundleInfo);
            }
        }

        /// <summary>
        /// 检查Bundle是否可以卸载，若可以则卸载
        /// </summary>
        internal static void CheckBundleLifeCycle(BundleRuntimeInfo bundleInfo)
        {
            if (bundleInfo.UsedAssets.Count == 0 && bundleInfo.DependencyCount == 0)
            {
                UnloadBundleTask task = new UnloadBundleTask(taskExcutor, bundleInfo.ManifestInfo.BundleName);
                taskExcutor.AddTask(task);
            }
        }

        #endregion




       



       


      


     


       


    }
}

