using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 合并资源清单与资源包
    /// </summary>
    public class MergeManifestAndBundles : IBuildTask
    {
        [InjectContext(ContextUsage.In)] 
        private IBundleBuildParameters buildParam;
        
        [InjectContext(ContextUsage.In)] 
        private IBundleBuildConfigParam configParam;
        
        [InjectContext(ContextUsage.In)] 
        private IManifestParam manifestParam;
        
        /// <inheritdoc />
        public int Version => 1;
        
        /// <inheritdoc />
        public ReturnCode Run()
        {
            BundleBuildConfigSO bundleBuildConfig = configParam.Config;
                BuildTarget targetPlatform = configParam.TargetPlatform;
                string directory = ((BundleBuildParameters) buildParam).OutputFolder;
                CatAssetManifest rawManifest = manifestParam.Manifest;
                
                //获取主资源清单(将清单版本号-1)
                string mainOutputPath = Util.GetFullOutputPath(bundleBuildConfig.OutputPath, targetPlatform,
                    bundleBuildConfig.ManifestVersion - 1);
                string mainManifestPath = Path.Combine(mainOutputPath,CatAsset.Runtime.Util.ManifestFileName);

                //尝试加载主资源清单
                CatAssetManifest mainManifest = null;
                if (File.Exists(mainManifestPath))
                {
                    string json = File.ReadAllText(mainManifestPath);
                    mainManifest = CatJson.JsonParser.ParseJson<CatAssetManifest>(json);
                }
                if (mainManifest == null)
                {
                    //没有主资源清单 不需要合并
                    return ReturnCode.SuccessNotRun;
                }
            
                foreach (BundleManifestInfo bundleManifestInfo in mainManifest.Bundles)
                {
                    if (!bundleManifestInfo.IsRaw)
                    {
                        //合并原生资源包
                        FileInfo fi = new FileInfo(Path.Combine(mainOutputPath, bundleManifestInfo.RelativePath));

                        string fullPath = Path.Combine(directory, bundleManifestInfo.RelativePath);
                        string fullDirectory =  Path.Combine(directory, bundleManifestInfo.Directory.ToLower());
                    
                        if (!Directory.Exists(fullDirectory))
                        {
                            //目录不存在则创建
                            Directory.CreateDirectory(fullDirectory);
                        }

                        fi.CopyTo(fullPath);
                    
                        //合并资源清单记录
                        rawManifest.Bundles.Add(bundleManifestInfo);
                    }
                }

                rawManifest.Bundles.Sort();
            
            return ReturnCode.Success;
        }

  
    }
}