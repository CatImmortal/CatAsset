using System;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 单机资源加载器
    /// </summary>
    public class PackageOnlyAssetLoader : DefaultAssetLoader
    {
        /// <inheritdoc />
        public override void CheckVersion(OnVersionChecked onVersionChecked)
        {
            string path = RuntimeUtil.GetReadOnlyPath(CatAssetManifest.ManifestBinaryFileName);

            CatAssetManager.AddWebRequestTask(path,path,((success, uwr) =>
            {
                string error = null;
                if (success)
                {
                    CatAssetManifest manifest = CatAssetManifest.DeserializeFromBinary(uwr.downloadHandler.data);
                    if (manifest != null)
                    {
                        CatAssetDatabase.InitRuntimeInfoByManifest(manifest);
                        Debug.Log("单机模式资源清单检查完毕");
                    }
                    else
                    {
                        success = false;
                        error = "资源清单校验失败";
                        Debug.LogError($"单机模式资源清单检查失败:{error}");
                    }
                }
                else
                {
                    error = uwr.error;
                    Debug.LogError($"单机模式资源清单检查失败:{error}");
                }
                
                var result = new VersionCheckResult(success, error, 0, 0);
                onVersionChecked?.Invoke(result);
                
                
                
            } ),TaskPriority.VeryLow);
        }
    }
}
