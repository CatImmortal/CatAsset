using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
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
        public static ReturnCode BuildBundles(BuildTarget targetPlatform,bool isOnlyBuildRaw)
        {
            BundleBuildConfigSO bundleBuildConfig = BundleBuildConfigSO.Instance;
            
            //预处理
            var preData = new BundleBuildPreProcessData
            {
                Config = bundleBuildConfig,
                TargetPlatform = targetPlatform
            };
            OnBundleBuildPreProcess(preData);
            
            string fullOutputPath = CreateFullOutputPath(bundleBuildConfig, targetPlatform);
            
            //准备参数
            BundleBuildParameters buildParam = GetParameters(bundleBuildConfig, targetPlatform, fullOutputPath);
            List<AssetBundleBuild> assetBundleBuilds = null;
            List<BundleBuildInfo> normalBundleBuilds = null;
            if (!isOnlyBuildRaw)
            {
                assetBundleBuilds = bundleBuildConfig.GetAssetBundleBuilds();
                normalBundleBuilds = bundleBuildConfig.GetNormalBundleBuilds();
            }
            else
            {
                assetBundleBuilds = new List<AssetBundleBuild>();
                normalBundleBuilds = new List<BundleBuildInfo>();
            }
            BundleBuildInfoParam infoParam = new BundleBuildInfoParam(assetBundleBuilds,
                normalBundleBuilds, bundleBuildConfig.GetRawBundleBuilds());
            BundleBuildConfigParam configParam =
                new BundleBuildConfigParam(bundleBuildConfig, targetPlatform);

            //开始构建资源包
            IBundleBuildResults result = null;
            ReturnCode returnCode;
            Stopwatch sw = Stopwatch.StartNew();
            if (!isOnlyBuildRaw)
            {
                returnCode = BuildBundles(buildParam, infoParam, configParam,out result);
            }
            else
            {
                returnCode = BuildRawBundles(buildParam, infoParam, configParam);
            }
            sw.Stop();
            
            //检查结果
            if (returnCode == ReturnCode.Success)
            {
                Debug.Log($"资源包构建成功:{returnCode},耗时:{sw.Elapsed.Hours}时{sw.Elapsed.Minutes}分{sw.Elapsed.Seconds}秒");
            }
            else
            {
                Debug.LogError($"资源包构建未成功:{returnCode},耗时:{sw.Elapsed.Hours}时{sw.Elapsed.Minutes}分{sw.Elapsed.Seconds}秒");
            }
        
            //后处理
            var postData = new BundleBuildPostProcessData
            {
                Config = bundleBuildConfig,
                TargetPlatform = targetPlatform,
                OutputFolder = fullOutputPath,
                ReturnCode = returnCode,
                Result = result,
            };
            OnBundleBuildPostProcess(postData);
            
            return returnCode;
            
        }

        /// <summary>
        /// 构建资源包
        /// </summary>
        private static ReturnCode BuildBundles(BundleBuildParameters buildParam,BundleBuildInfoParam infoParam,BundleBuildConfigParam configParam,out IBundleBuildResults result)
        {
            BundleBuildConfigSO bundleBuildConfig = BundleBuildConfigSO.Instance;
            BundleBuildContent content = new BundleBuildContent(infoParam.AssetBundleBuilds);
            
            //添加构建任务
            List<IBuildTask> taskList = GetSBPInternalBuildTask();
            taskList.Add(new BuildRawBundles());
            taskList.Add(new BuildManifest());
            taskList.Add(new EncryptBundles());
            taskList.Add(new CalculateVerifyInfo());
            taskList.Add(new AppendMD5());
            taskList.Add(new WriteManifestFile());
            if (bundleBuildConfig.IsCopyToReadOnlyDirectory && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                taskList.Add(new CopyToReadOnlyDirectory());
                taskList.Add(new WriteManifestFile());
            }
            
            //调用SBP的构建管线
            ReturnCode returnCode = ContentPipeline.BuildAssetBundles(buildParam, content,
                out result, taskList, infoParam,configParam);

            return returnCode;
        }

        /// <summary>
        /// 仅构建原生资源包
        /// </summary>
        private static ReturnCode BuildRawBundles(BundleBuildParameters buildParam,BundleBuildInfoParam infoParam,BundleBuildConfigParam configParam)
        {
            BundleBuildConfigSO bundleBuildConfig = BundleBuildConfigSO.Instance;
            BundleBuildResults results = new BundleBuildResults();
            BuildContext buildContext = new BuildContext(buildParam,infoParam,configParam,results);
            
            //添加构建任务
            IList<IBuildTask> taskList = new List<IBuildTask>();
            taskList.Add(new BuildRawBundles());
            taskList.Add(new BuildManifest());
            taskList.Add(new EncryptBundles());
            taskList.Add(new CalculateVerifyInfo());
            taskList.Add(new AppendMD5());
            taskList.Add(new MergeManifestAndBundles());
            taskList.Add(new WriteManifestFile());
            if (bundleBuildConfig.IsCopyToReadOnlyDirectory && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                taskList.Add(new CopyToReadOnlyDirectory());
                taskList.Add(new WriteManifestFile());
            }
            
            //运行构建任务
            ReturnCode returnCode = BuildTasksRunner.Run(taskList, buildContext);

            return returnCode;
        }

        /// <summary>
        /// 构建资源包前调用
        /// </summary>
        private static void OnBundleBuildPreProcess(BundleBuildPreProcessData data)
        {
            List<IBundleBuildPreProcessor> objs = EditorUtil.GetAssignableTypeObjects<IBundleBuildPreProcessor>();
            foreach (var obj in objs)
            {
                obj.OnBundleBuildPreProcess(data);
            }
        }

        /// <summary>
        /// 构建资源包后调用
        /// </summary>
        private static void OnBundleBuildPostProcess(BundleBuildPostProcessData data)
        {
            List<IBundleBuildPostProcessor> objs = EditorUtil.GetAssignableTypeObjects<IBundleBuildPostProcessor>();
            foreach (var obj in objs)
            {
                obj.OnBundleBuildPostProcess(data);
            }
        }

        /// <summary>
        /// 是否包含目标资源包构建设置
        /// </summary>
        private static bool HasOption(BundleBuildOptions options, BundleBuildOptions target)
        {
            return (options & target) != 0;
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

            //全局压缩格式
            switch (bundleBuildConfig.GlobalCompress)
            {
                case BundleCompressOptions.Uncompressed:
                    parameters.BundleCompression = BuildCompression.Uncompressed;
                    break;
                
                case BundleCompressOptions.LZ4:
                    parameters.BundleCompression = BuildCompression.LZ4;
                    break;
                
                case BundleCompressOptions.LZMA:
                    parameters.BundleCompression = BuildCompression.LZMA;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return parameters;
        }
        

        /// <summary>
        /// 获取SBP内置的构建任务
        /// </summary>
        private static List<IBuildTask> GetSBPInternalBuildTask()
        {
            var buildTasks = new List<IBuildTask>();

            // Setup
            buildTasks.Add(new SwitchToBuildPlatform());
            buildTasks.Add(new RebuildSpriteAtlasCache());

            // Player Scripts
            buildTasks.Add(new BuildPlayerScripts());
            buildTasks.Add(new PostScriptsCallback());

            // Dependency
            buildTasks.Add(new CalculateSceneDependencyData());
#if UNITY_2019_3_OR_NEWER
            buildTasks.Add(new CalculateCustomDependencyData());
#endif
            buildTasks.Add(new CalculateAssetDependencyData());
            buildTasks.Add(new StripUnusedSpriteSources());
            buildTasks.Add(new CreateBuiltInShadersBundle(RuntimeUtil.BuiltInShaderBundleName));
            buildTasks.Add(new PostDependencyCallback());

            // Packing
            buildTasks.Add(new GenerateBundlePacking());
            buildTasks.Add(new FixSpriteAtlasBug());  //这里插入一个修复SBP图集Bug的任务
            buildTasks.Add(new UpdateBundleObjectLayout());
            buildTasks.Add(new GenerateBundleCommands());
            buildTasks.Add(new GenerateSubAssetPathMaps());
            buildTasks.Add(new GenerateBundleMaps());
            buildTasks.Add(new PostPackingCallback());

            // Writing
            buildTasks.Add(new WriteSerializedFiles());
            buildTasks.Add(new ArchiveAndCompressBundles());
            buildTasks.Add(new AppendBundleHash());
            buildTasks.Add(new GenerateLinkXml());
            buildTasks.Add(new PostWritingCallback());

            return buildTasks;
        }
    }
}
