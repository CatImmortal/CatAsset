using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源管理器
    /// </summary>
    public static class CatAssetManager
    {
        private static TaskRunner loadTaskRunner = new TaskRunner();
        private static TaskRunner downloadTaskRunner = new TaskRunner();
        
        /// <summary>
        /// 资源包相对路径->资源包运行时信息（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        internal static readonly Dictionary<string, BundleRuntimeInfo> bundleInfoDict = new Dictionary<string, BundleRuntimeInfo>();
        
        /// <summary>
        /// 资源名->资源运行时信息（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        internal static readonly Dictionary<string, AssetRuntimeInfo> assetInfoDict = new Dictionary<string, AssetRuntimeInfo>();
        
        /// <summary>
        /// 加载模式
        /// </summary>
        public static RuntimeMode RuntimeMode
        {
            get;
            set;
        }
        
        /// <summary>
        /// 是否开启编辑器资源模式
        /// </summary>
        public static bool IsEditorMode
        {
            get;
            set;
        }
        
        /// <summary>
        /// 轮询CatAsset管理器
        /// </summary>
        public static void Update()
        {
            loadTaskRunner.Update();
            downloadTaskRunner.Update();
        }
        
        /// <summary>
        /// 根据资源包清单信息初始化运行时信息
        /// </summary>
        internal static void InitRuntimeInfo(BundleManifestInfo bundleManifestInfo, bool inReadWrite)
        {
            BundleRuntimeInfo bundleRuntimeInfo = new BundleRuntimeInfo();
            bundleInfoDict.Add(bundleManifestInfo.RelativePath, bundleRuntimeInfo);
            bundleRuntimeInfo.BundleManifest = bundleManifestInfo;
            bundleRuntimeInfo.InReadWrite = inReadWrite;

            foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
            {
                AssetRuntimeInfo assetRuntimeInfo = new AssetRuntimeInfo();
                assetInfoDict.Add(assetManifestInfo.AssetName, assetRuntimeInfo);
                assetRuntimeInfo.BundleManifest = bundleManifestInfo;
                assetRuntimeInfo.AssetManifest = assetManifestInfo;
            }
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

            WebRequestTask task = new WebRequestTask(downloadTaskRunner, path, path, callback,
                (success, error, uwr, userdata) =>
                {
                    Action<bool> onChecked = (Action<bool>)userdata;

                    if (!success)
                    {
                        Debug.LogError($"单机模式资源清单检查失败:{error}");
                        onChecked?.Invoke(false);
                    }
                    else
                    {
                        CatAssetManifest manifest = CatJson.JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);
                        foreach (BundleManifestInfo info in manifest.Bundles)
                        {
                            InitRuntimeInfo(info, false);
                        }
                        Debug.Log("单机模式资源清单检查完毕");
                        onChecked?.Invoke(true);
                    }
                });
            
           downloadTaskRunner.AddTask(task,TaskPriority.Height);
        }


    }
}

