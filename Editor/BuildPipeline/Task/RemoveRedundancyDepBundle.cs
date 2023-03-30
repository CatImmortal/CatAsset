using System.IO;
using CatAsset.Runtime;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 删除构建补丁包时产生的冗余依赖包
    /// </summary>
    public class RemoveRedundancyDepBundle : IBuildTask
    {
        public int Version { get; }
        
        [InjectContext(ContextUsage.InOut)]
        private IBundleBuildInfoParam buildInfoParam;
        
        [InjectContext(ContextUsage.In)]
        private IBundleBuildParameters buildParam;
        
        public ReturnCode Run()
        {
            for (int i = 0; i < buildInfoParam.NormalBundleBuilds.Count; i++)
            {
                var buildInfo = buildInfoParam.NormalBundleBuilds[i];
                if (buildInfo.BundleName == EditorUtil.RedundancyDepBundleName)
                {
                    buildInfoParam.NormalBundleBuilds.RemoveAt(i);
                    buildInfoParam.AssetBundleBuilds.RemoveAt(i);
                    
                    string outputFolder = ((BundleBuildParameters) buildParam).OutputFolder;
                    string path = RuntimeUtil.GetRegularPath(Path.Combine(outputFolder, EditorUtil.RedundancyDepBundleName));
                    File.Delete(path);
                    return ReturnCode.Success;
                }
            }

            return ReturnCode.SuccessNotRun;
        }
    }
}