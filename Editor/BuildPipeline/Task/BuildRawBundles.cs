using System.Collections.Generic;
using System.IO;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 构建原生资源包
    /// </summary>
    public class BuildRawBundles : IBuildTask
    {
        [InjectContext(ContextUsage.In)]
        private IBundleBuildParameters buildParam;
        
        [InjectContext(ContextUsage.In)]
        private IBundleBuildInfoParam infoParam;
        
        /// <inheritdoc />
        public int Version => 1;

        /// <inheritdoc />
        public ReturnCode Run()
        {
            
            string directory = ((BundleBuildParameters)buildParam).OutputFolder;

            List<BundleBuildInfo> rawBundleBuilds = infoParam.RawBundleBuilds;
                
            if (rawBundleBuilds == null)
            {
                return ReturnCode.SuccessNotRun;
            }
                
            //遍历原生资源包列表
            foreach (BundleBuildInfo rawBundleBuildInfo in rawBundleBuilds)
            {
                string rawAssetName = rawBundleBuildInfo.Assets[0].Name;
                string rawBundleDirectory = Path.Combine(directory, rawBundleBuildInfo.DirectoryName.ToLower());
                if (!Directory.Exists(rawBundleDirectory))
                {
                    Directory.CreateDirectory(rawBundleDirectory);
                }

                string targetFileName = Path.Combine(directory, rawBundleBuildInfo.RelativePath);
                File.Copy(rawAssetName, targetFileName); //直接将原生资源复制过去
            }

            return ReturnCode.Success;
        }
    }
}