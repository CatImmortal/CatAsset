using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine.Build.Pipeline;

namespace CatAsset.Editor
{
    /// <summary>
    /// 计算校验信息
    /// </summary>
    public class CalculateVerifyInfo : IBuildTask
    {
        [InjectContext(ContextUsage.In)]
        private IManifestParam manifestParam;

        [InjectContext(ContextUsage.In)]
        private IBundleBuildParameters buildParam;

        [InjectContext(ContextUsage.In)] 
        private IBundleBuildConfigParam configParam;
        
        [InjectContext(ContextUsage.In)]
        private IBundleBuildResults results;
        
        public int Version { get; }

        public ReturnCode Run()
        {
            CatAssetManifest manifest = manifestParam.Manifest;
            string outputFolder = ((BundleBuildParameters) buildParam).OutputFolder;

            foreach (BundleManifestInfo bundleManifestInfo in manifest.Bundles)
            {
                string path = Path.Combine(outputFolder,  bundleManifestInfo.BundleIdentifyName);
                
                //计算文件长度与MD5
                FileInfo fi = new FileInfo(path);
                bundleManifestInfo.Length = (ulong)fi.Length;
                bundleManifestInfo.MD5 = RuntimeUtil.GetFileMD5(path);
                
                if (!bundleManifestInfo.IsRaw)
                {
                    //不是原生资源 记录Hash
                    BundleDetails details = results.BundleInfos[bundleManifestInfo.BundleIdentifyName];
                    bundleManifestInfo.Hash = details.Hash.ToString();
                }
                else
                {
                    //原生资源就将MD5视为Hash处理
                    bundleManifestInfo.Hash = bundleManifestInfo.MD5;
                }
            }

            return ReturnCode.Success;
        }
    }
}