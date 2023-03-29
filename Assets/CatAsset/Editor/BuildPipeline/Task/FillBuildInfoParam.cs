using System.IO;
using CatAsset.Runtime;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 填充资源包构建信息参数
    /// </summary>
    public class FillBuildInfoParam : IBuildTask
    {
        public int Version { get; }
        
        [InjectContext(ContextUsage.In)]
        private IBundleBuildParameters buildParam;

        [InjectContext(ContextUsage.InOut)]
        private IBundleBuildInfoParam buildInfoParam;
        
        [InjectContext(ContextUsage.In)]
        private IBundleBuildConfigParam configParam;
        
        [InjectContext(ContextUsage.InOut)]
        private IBundleBuildContent content;

        [InjectContext(ContextUsage.InOut)]
        private IBuildContent content2;
        
        public ReturnCode Run()
        {
            BundleBuildConfigSO config = configParam.Config;
            if (!configParam.IsBuildPatch)
            {
                //构建完整资源包
                buildInfoParam = new BundleBuildInfoParam(config.GetAssetBundleBuilds(),config.GetNormalBundleBuilds(),
                    config.GetRawBundleBuilds());
            }
            else
            {
                //构建补丁资源包
                var folder = EditorUtil.GetManifestCacheFolder(config.OutputRootDirectory, configParam.TargetPlatform);
                string path = RuntimeUtil.GetRegularPath(Path.Combine(folder, CatAssetManifest.ManifestBinaryFileName));
                CatAssetManifest cacheManifest = CatAssetManifest.DeserializeFromBinary(File.ReadAllBytes(path));
            }

            ((BundleBuildParameters)buildParam).SetBundleBuilds(buildInfoParam.NormalBundleBuilds);
            content = new BundleBuildContent(buildInfoParam.AssetBundleBuilds);
            content2 = content;
            
            return ReturnCode.Success;
        }
    }
}