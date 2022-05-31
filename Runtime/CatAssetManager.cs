using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源管理器
    /// </summary>
    public static class CatAssetManager
    {
        /// <summary>
        /// 下载相关任务运行器
        /// </summary>
        private static TaskRunner downloadTaskRunner = new TaskRunner();
        
        /// <summary>
        /// 加载相关任务运行器
        /// </summary>
        private static TaskRunner loadTaskRunner = new TaskRunner();



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
        /// 资源对象->资源运行时信息
        /// </summary>
        private static Dictionary<object, AssetRuntimeInfo> assetDict = new Dictionary<object, AssetRuntimeInfo>();

        /// <summary>
        /// 运行模式
        /// </summary>
        public static RuntimeMode RuntimeMode { get; set; }

        /// <summary>
        /// 是否开启编辑器资源模式
        /// </summary>
        public static bool IsEditorMode { get; set; }

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
        public static BundleRuntimeInfo GetBundleRuntimeInfo(string bundleRelativePath)
        {
            return bundleRuntimeInfoDict[bundleRelativePath];
        }

        /// <summary>
        /// 获取资源运行时信息
        /// </summary>
        public static AssetRuntimeInfo GetAssetRuntimeInfo(string assetName)
        {
            return assetRuntimeInfoDict[assetName];
        }

        /// <summary>
        /// 获取资源运行时信息
        /// </summary>
        public static AssetRuntimeInfo GetAssetRuntimeInfo(object asset)
        {
            return assetDict[asset];
        }

        /// <summary>
        /// 设置资源运行时信息
        /// </summary>
        public static void SetAssetRuntimeInfo(object asset, AssetRuntimeInfo assetRuntimeInfo)
        {
            assetDict.Add(asset, assetRuntimeInfo);
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
        /// 轮询CatAsset资源管理器
        /// </summary>
        public static void Update()
        {
            loadTaskRunner.Update();
            downloadTaskRunner.Update();
        }

        /// <summary>
        /// 检查安装包内资源清单,仅使用安装包内资源模式下专用
        /// </summary>
        public static void CheckPackageManifest(Action<bool> callback)
        {
            if (RuntimeMode != RuntimeMode.PackageOnly)
            {
                Debug.LogError("PackageOnly模式下才能调用CheckPackageManifest");
                return;
            }

            string path = Util.GetReadOnlyPath(Util.ManifestFileName);

            WebRequestTask task = WebRequestTask.Create(downloadTaskRunner, path, path, callback,
                (success, error, uwr, userdata) =>
                {
                    Action<bool> onChecked = (Action<bool>) userdata;

                    if (!success)
                    {
                        Debug.LogError($"单机模式资源清单检查失败:{error}");
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

            downloadTaskRunner.AddTask(task, TaskPriority.Height);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static void LoadAsset(string assetName, object userdata, LoadAssetTaskCallback<Object> callback,
            TaskPriority priority = TaskPriority.Low)
        {
            LoadAsset<Object>(assetName, userdata, callback, priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static void LoadAsset<T>(string assetName, object userdata, LoadAssetTaskCallback<T> callback,
            TaskPriority priority = TaskPriority.Low) where T : Object
        {
            //检查资源是否已在本地准备好
            if (!CheckAssetReady(assetName))
            {
                return;
            }

            AssetRuntimeInfo info = assetRuntimeInfoDict[assetName];
            if (info.Asset != null)
            {
                //此资源已加载过了
                //增加引用计数后直接返回
                info.RefCount++;
                callback?.Invoke(true, (T) info.Asset, userdata);
                return;
            }

            Type assetType = typeof(T);
            if (assetType != info.AssetManifest.Type && assetType != typeof(Object))
            {
                Debug.LogError(
                    $"资源加载类型错误，资源名:{info.AssetManifest.Name},资源类型:{info.AssetManifest.Type},目标类型:{typeof(T).Name}");
                return;
            }

            //未被加载过 开始加载
            LoadAssetTask<T> task = LoadAssetTask<T>.Create(loadTaskRunner, assetName, userdata, callback);
            loadTaskRunner.AddTask(task, priority);
        }


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

            if (!assetDict.TryGetValue(asset, out AssetRuntimeInfo assetInfo))
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

            //InternalUnloadAsset(assetInfo);
        }
    }
}