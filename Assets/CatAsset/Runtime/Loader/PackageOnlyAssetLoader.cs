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
            string path = RuntimeUtil.GetReadOnlyPath(RuntimeUtil.ManifestFileName);

            CatAssetManager.AddWebRequestTask(path,path,((success, uwr) =>
            {
                VersionCheckResult result = default;
                if (!success)
                {
                    Debug.LogError($"单机模式资源清单检查失败:{uwr.error}");
                    result = new VersionCheckResult(false, uwr.error, 0, 0);
                }
                else
                {
                    CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(uwr.downloadHandler.text);
                    CatAssetDatabase.InitRuntimeInfoByManifest(manifest);
                    Debug.Log("单机模式资源清单检查完毕");
                    result = new VersionCheckResult(true, null, 0, 0);
                }
                
                onVersionChecked?.Invoke(result);
            } ),TaskPriority.VeryLow);
        }
    }
}