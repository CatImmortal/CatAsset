using System;
using System.IO;
using UnityEngine;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 检查资源版本
        /// </summary>
        public static void CheckVersion(OnVersionChecked onVersionChecked)
        {
            assetLoader.CheckVersion(onVersionChecked);
        }

        /// <summary>
        /// 从外部导入额外的读写区资源清单
        /// </summary>
        public static void ImportReadWriteManifest(string manifestPath, Action<bool> callback,
            string bundleRelativePathPrefix = null)
        {
            manifestPath = RuntimeUtil.GetReadWritePath(manifestPath,true);
            
            AddWebRequestTask(manifestPath,manifestPath,((success, uwr) =>
            {
                if (!success)
                {
                    Debug.LogError($"资源导入失败:{uwr.error}");
                    callback?.Invoke(false);
                }
                else
                {
                    CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(uwr.downloadHandler.text);
        
                    foreach (BundleManifestInfo bundleManifestInfo in manifest.Bundles)
                    {
                        if (!string.IsNullOrEmpty(bundleRelativePathPrefix))
                        {
                            //为资源包目录名添加额外前缀
                            bundleManifestInfo.Directory = RuntimeUtil.GetRegularPath(Path.Combine(bundleRelativePathPrefix,
                                bundleManifestInfo.Directory));
                        }
        
                        CatAssetDatabase.InitRuntimeInfo(bundleManifestInfo,BundleRuntimeInfo.State.InReadWrite);
                    }
        
                    Debug.Log("内置资源导入完毕");
                    callback?.Invoke(true);
                }
                
            }),TaskPriority.Height);
        }
    }
}