using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CatAsset.Editor
{
    /// <summary>
    /// 计算资源包构建信息
    /// </summary>
    public class CalculateBundleBuilds : IBuildTask
    {
        public int Version { get; }


        [InjectContext(ContextUsage.In)] 
        private IBundleBuildParameters buildParam;

        [InjectContext(ContextUsage.InOut)] 
        private IBundleBuildInfoParam buildInfoParam;

        [InjectContext(ContextUsage.In)] 
        private IBundleBuildConfigParam configParam;

        [InjectContext(ContextUsage.In)] 
        private IBuildCache buildCache;

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
                buildInfoParam = new BundleBuildInfoParam(config.GetAssetBundleBuilds(), config.GetNormalBundleBuilds(),
                    config.GetRawBundleBuilds());
            }
            else
            {
                //构建补丁资源包
                Stopwatch sw = Stopwatch.StartNew();
                
                var clonedConfig = new PatchAssetCalculateHelper().Calculate(config, configParam.TargetPlatform);

                sw.Stop();
                Debug.Log($"计算补丁资源耗时:{sw.Elapsed.TotalSeconds:0.00}秒");
                
                buildInfoParam = new BundleBuildInfoParam(clonedConfig.GetAssetBundleBuilds(),
                    clonedConfig.GetNormalBundleBuilds(),
                    clonedConfig.GetRawBundleBuilds());
            }

            ((BundleBuildParameters)buildParam).SetBundleBuilds(buildInfoParam.NormalBundleBuilds);
            content = new BundleBuildContent(buildInfoParam.AssetBundleBuilds);
            content2 = content;

            return ReturnCode.Success;
        }


       

    }
}