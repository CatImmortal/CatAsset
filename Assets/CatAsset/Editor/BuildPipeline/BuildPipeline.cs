using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using BuildCompression = UnityEngine.BuildCompression;
using Debug = UnityEngine.Debug;

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
            IList<IBuildTask> taskList = DefaultBuildTasks.Create(DefaultBuildTasks.Preset.AssetBundleBuiltInShaderExtraction);
            taskList.Add(new BuildRawBundles());
            taskList.Add(new BuildManifest());
            if (HasOption(bundleBuildConfig.Options,BundleBuildOptions.AppendMD5))
            {
                taskList.Add(new AppendMD5());
            }
            taskList.Add(new WriteManifestFile());
            if (bundleBuildConfig.IsCopyToReadOnlyDirectory && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                taskList.Add(new CopyToReadOnlyDirectory());
                taskList.Add(new WriteManifestFile());
            }

            Stopwatch sw = Stopwatch.StartNew();
            //调用SBP的构建管线
            ReturnCode returnCode = ContentPipeline.BuildAssetBundles(buildParam, content,
                out IBundleBuildResults result, taskList, infoParam,configParam);
            sw.Stop();
            Debug.Log($"资源包构建结束:{returnCode},耗时:{sw.Elapsed.Hours}时{sw.Elapsed.Minutes}分{sw.Elapsed.Seconds}秒");
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
            if (HasOption(bundleBuildConfig.Options,BundleBuildOptions.AppendMD5))
            {
                taskList.Add(new AppendMD5());
            }
            taskList.Add(new MergeManifestAndBundles());
            taskList.Add(new WriteManifestFile());
            if (bundleBuildConfig.IsCopyToReadOnlyDirectory && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                taskList.Add(new CopyToReadOnlyDirectory());
                taskList.Add(new WriteManifestFile());
            }
            
            Stopwatch sw = Stopwatch.StartNew();
            //运行构建任务
            BuildTasksRunner.Run(taskList, buildContext);
            
            Debug.Log($"原生资源包构建结束，耗时:{sw.Elapsed.Hours}时{sw.Elapsed.Minutes}分{sw.Elapsed.Seconds}秒");
        }
        
        /// <summary>
        /// 是否包含目标资源包构建设置
        /// </summary>
        private static bool HasOption(BundleBuildOptions options, BundleBuildOptions target)
        {
            return (options & target) != 0;
        }
        
        /// <summary>
        /// 获取SBP用到的构建参数
        /// </summary>
        private static BundleBuildParameters GetParameters(BundleBuildConfigSO bundleBuildConfig,
            BuildTarget targetPlatform, string fullOutputPath)
        {
            BuildTargetGroup group = UnityEditor.BuildPipeline.GetBuildTargetGroup(targetPlatform);

            BundleBuildParameters parameters = new BundleBuildParameters(targetPlatform, group, fullOutputPath);

            //是否生成LinkXML
            parameters.WriteLinkXML = HasOption(bundleBuildConfig.Options,BundleBuildOptions.WriteLinkXML);
            
            //是否增量构建
            parameters.UseCache = !HasOption(bundleBuildConfig.Options,BundleBuildOptions.ForceRebuild);
           
            //是否使用LZ4压缩
            if (HasOption(bundleBuildConfig.Options,BundleBuildOptions.ChunkBasedCompression))
            {
                parameters.BundleCompression = BuildCompression.LZ4;
            }
            else
            {
                parameters.BundleCompression = BuildCompression.Uncompressed;
            }
            
            return parameters;
        }

        /// <summary>
        /// 创建完整资源包构建输出目录
        /// </summary>
        private static string CreateFullOutputPath(BundleBuildConfigSO bundleBuildConfig, BuildTarget targetPlatform)
        {
            string fullOutputPath = EditorUtil.GetFullOutputPath(bundleBuildConfig.OutputPath, targetPlatform,
                bundleBuildConfig.ManifestVersion);
            EditorUtil.CreateEmptyDirectory(fullOutputPath);
            return fullOutputPath;
        }
    }
}