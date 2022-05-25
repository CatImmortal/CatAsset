using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 构建管线
    /// </summary>
    public static class BuildPipeline
    {
        /// <summary>
        /// 构建资源包
        /// </summary>
        public static TaskResult BuildBundles(BundleBuildConfigSO bundleBuildConfig, BuildTarget targetPlatform)
        {
            BundleBuildConfigParam bundleBuildConfigParam = new BundleBuildConfigParam()
            {
                Config = bundleBuildConfig,
                TargetPlatform = targetPlatform,
            };
            
            BundleBuildsParam bundleBuildsParam = new BundleBuildsParam()
            {
                AssetBundleBuilds = bundleBuildConfig.GetAssetBundleBuilds(),
                NormalBundleBuilds = bundleBuildConfig.GetNormalBundleBuilds(),
                RawBundleBuilds = bundleBuildConfig.GetRawBundleBuilds(),
            };
            
            //注入构建管线参数
            BuildPipelineRunner.InjectParam(bundleBuildConfigParam);
            BuildPipelineRunner.InjectParam(bundleBuildsParam);

            //创建任务列表
            List<IBuildPipelineTask> tasks = new List<IBuildPipelineTask>()
            {
                new CreateOutputDirectoryTask(),
                new BuildAssetBundleTask(),
                new DeleteUnityManifestFileTask(),
                new BuildRawBundleTask(),
                new CreateManifestTask(),
                new WriteManifestFileTask(),
            };
            
            if (bundleBuildConfig.IsCopyToReadOnlyPath && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                tasks.Add(new CopyToReadOnlyDirectoryTask());
                tasks.Add(new WriteManifestFileTask());
            }
            
            //运行构建管线任务
            return BuildPipelineRunner.Run(tasks);
        }

        /// <summary>
        /// 构建原生资源包
        /// </summary>
        public static TaskResult BuildRawBundles(BundleBuildConfigSO bundleBuildConfig,
            BuildTarget targetPlatform)
        {
            BundleBuildConfigParam bundleBuildConfigParam = new BundleBuildConfigParam()
            {
                Config = bundleBuildConfig,
                TargetPlatform = targetPlatform,
            };
            
            BundleBuildsParam bundleBuildsParam = new BundleBuildsParam()
            {
                AssetBundleBuilds = new List<AssetBundleBuild>(),
                NormalBundleBuilds = new List<BundleBuildInfo>(),
                RawBundleBuilds = bundleBuildConfig.GetRawBundleBuilds(),
            };
            
            //注入构建管线参数
            BuildPipelineRunner.InjectParam(bundleBuildConfigParam);
            BuildPipelineRunner.InjectParam(bundleBuildsParam);
            BuildPipelineRunner.InjectParam(new UnityManifestParam());  //给个空参数
            
            //创建任务列表
            List<IBuildPipelineTask> tasks = new List<IBuildPipelineTask>()
            {
                new CreateOutputDirectoryTask(),
                new BuildRawBundleTask(),
                new CreateManifestTask(),
                new MergeManifestAndBundlesTask(), //仅构建原生资源包的情况，需要合并主资源清单和主资源包
                new WriteManifestFileTask(),
            };
            
            if (bundleBuildConfig.IsCopyToReadOnlyPath && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                tasks.Add(new CopyToReadOnlyDirectoryTask());
                tasks.Add(new WriteManifestFileTask());
            }
            
            //运行构建管线任务
            return BuildPipelineRunner.Run(tasks);
        }
        
        
    }
}