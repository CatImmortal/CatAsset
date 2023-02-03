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
                
                if (configParam.TargetPlatform == BuildTarget.WebGL && !bundleManifestInfo.IsRaw)
                {
                    //WebGL平台 且不是原生资源包 记录Hash128用于缓存系统
                    BundleDetails details = results.BundleInfos[bundleManifestInfo.BundleIdentifyName];
                    bundleManifestInfo.Hash = details.Hash.ToString();
                }
            }

            return ReturnCode.Success;
        }
    }
}