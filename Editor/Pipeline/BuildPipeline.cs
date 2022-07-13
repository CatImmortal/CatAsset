using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using BuildCompression = UnityEngine.BuildCompression;

namespace CatAsset.Editor
{
    /// <summary>
    /// 构建管线
    /// </summary>
    public static class BuildPipeline
    {
        /// <summary>
        /// 获取SBP用到的构建参数
        /// </summary>
        private static BundleBuildParameters GetParameters(BundleBuildConfigSO bundleBuildConfig,
            BuildTarget targetPlatform, string fullOutputPath)
        {
            BuildTargetGroup group = UnityEditor.BuildPipeline.GetBuildTargetGroup(targetPlatform);

            BundleBuildParameters parameters = new BundleBuildParameters(targetPlatform, group, fullOutputPath);

            //是否全量构建
            if ((bundleBuildConfig.Options & BuildAssetBundleOptions.ForceRebuildAssetBundle) != 0)
            {
                parameters.UseCache = false;
            }

            //是否附加hash值到文件名
            if ((bundleBuildConfig.Options & BuildAssetBundleOptions.AppendHashToAssetBundleName) != 0)
            {
                parameters.AppendHash = true;
            }

            //压缩格式
            if ((bundleBuildConfig.Options & BuildAssetBundleOptions.ChunkBasedCompression) != 0)
            {
                parameters.BundleCompression = BuildCompression.LZ4;
            }
            else if ((bundleBuildConfig.Options & BuildAssetBundleOptions.UncompressedAssetBundle) != 0)
            {
                parameters.BundleCompression = BuildCompression.Uncompressed;
            }
            else
            {
                parameters.BundleCompression = BuildCompression.LZMA;
            }

            //TypeTree
            if ((bundleBuildConfig.Options & BuildAssetBundleOptions.DisableWriteTypeTree) != 0)
            {
                parameters.ContentBuildFlags |= ContentBuildFlags.DisableWriteTypeTree;
            }

            return parameters;
        }

        /// <summary>
        /// 创建完整资源包构建输出目录
        /// </summary>
        private static string CreateFullOutputPath(BundleBuildConfigSO bundleBuildConfig, BuildTarget targetPlatform)
        {
            string fullOutputPath = Util.GetFullOutputPath(bundleBuildConfig.OutputPath, targetPlatform,
                bundleBuildConfig.ManifestVersion);
            Util.CreateEmptyDirectory(fullOutputPath);
            return fullOutputPath;
        }

        /// <summary>
        /// 构建资源包
        /// </summary>
        public static void BuildBundles(BundleBuildConfigSO bundleBuildConfig, BuildTarget targetPlatform)
        {
            string fullOutputPath = CreateFullOutputPath(bundleBuildConfig, targetPlatform);

            //准备参数
            BundleBuildParameters buildParam = GetParameters(bundleBuildConfig, targetPlatform, fullOutputPath);
            BundleBuildInfoParam infoParam = new BundleBuildInfoParam(bundleBuildConfig.GetAssetBundleBuilds(),
                bundleBuildConfig.GetNormalBundleBuilds(), bundleBuildConfig.GetRawBundleBuilds());
            BundleBuildConfigParam configParam =
                new BundleBuildConfigParam(bundleBuildConfig, targetPlatform);
            
            BundleBuildContent content = new BundleBuildContent(infoParam.AssetBundleBuilds);

            //添加构建任务
            IList<IBuildTask> taskList = DefaultBuildTasks.Create(DefaultBuildTasks.Preset.AssetBundleCompatible);
            taskList.Add(new BuildRawBundles());
            taskList.Add(new BuildManifest());
            taskList.Add(new WriteManifestFile());
            if (bundleBuildConfig.IsCopyToReadOnlyDirectory && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                taskList.Add(new CopyToReadOnlyDirectory());
                taskList.Add(new WriteManifestFile());
            }

            //调用SBP的构建管线
            ReturnCode returnCode = ContentPipeline.BuildAssetBundles(buildParam, content,
                out IBundleBuildResults result, taskList, infoParam,configParam);

            Debug.Log("资源包构建结束");
        }

        /// <summary>
        /// 构建原生资源包
        /// </summary>
        public static void BuildRawBundles(BundleBuildConfigSO bundleBuildConfig,
            BuildTarget targetPlatform)
        {
            string fullOutputPath = CreateFullOutputPath(bundleBuildConfig, targetPlatform);
            
            //准备参数
            BundleBuildParameters buildParam = GetParameters(bundleBuildConfig, targetPlatform, fullOutputPath);
            BundleBuildInfoParam infoParam = new BundleBuildInfoParam(new List<AssetBundleBuild>(),
                new List<BundleBuildInfo>(), bundleBuildConfig.GetRawBundleBuilds());
            BundleBuildConfigParam configParam =
                new BundleBuildConfigParam(bundleBuildConfig, targetPlatform);
            BundleBuildResults results = new BundleBuildResults();  //这里给个空参数，不然会报错
            
            BuildContext buildContext = new BuildContext(buildParam,infoParam,configParam,results);
            
            //添加构建任务
            IList<IBuildTask> taskList = new List<IBuildTask>();
            taskList.Add(new BuildRawBundles());
            taskList.Add(new BuildManifest());
            taskList.Add(new MergeManifestAndBundles());
            taskList.Add(new WriteManifestFile());
            if (bundleBuildConfig.IsCopyToReadOnlyDirectory && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                taskList.Add(new CopyToReadOnlyDirectory());
                taskList.Add(new WriteManifestFile());
            }
            
            //运行构建任务
            BuildTasksRunner.Run(taskList, buildContext);
            
            Debug.Log("原生资源包构建结束");
        }
    }
}