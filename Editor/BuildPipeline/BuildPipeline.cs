using System;
using System.Collections.Generic;
using System.Diagnostics;
using CatAsset.Runtime;
using UnityEditor;
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
        public static ReturnCode BuildBundles(BuildTarget targetPlatform,bool isBuildPatch)
        {
            BundleBuildConfigSO bundleBuildConfig = BundleBuildConfigSO.Instance;
            
            //预处理
            var preData = new BundleBuildPreProcessData
            {
                Config = bundleBuildConfig,
                TargetPlatform = targetPlatform
            };
            OnBundleBuildPreProcess(preData);
            
            string outputFolder = CreateFullOutputFolder(bundleBuildConfig, targetPlatform);
            
            //准备参数
            BundleBuildInfoParam buildInfoParam = new BundleBuildInfoParam();
            BundleBuildConfigParam configParam = new BundleBuildConfigParam(bundleBuildConfig, targetPlatform,isBuildPatch);
            BundleBuildParameters buildParam = GetSBPParameters(bundleBuildConfig, targetPlatform, outputFolder);
            BundleBuildContent content = new BundleBuildContent();
            
            //开始构建资源包
            Stopwatch sw = Stopwatch.StartNew();
            
            //添加构建任务
            List<IBuildTask> taskList = GetSBPInternalBuildTask();
            taskList.Insert(0,new FillBuildInfoParam());
            taskList.Add(new BuildRawBundles());
            taskList.Add(new BuildManifest());
            taskList.Add(new EncryptBundles());
            taskList.Add(new CalculateVerifyInfo());
            taskList.Add(new AppendMD5());
            taskList.Add(new WriteManifestFile());
            taskList.Add(new WriteManifestFileToCache());
            taskList.Add(new WriteManifestFile());
            if (bundleBuildConfig.IsCopyToReadOnlyDirectory && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                taskList.Add(new CopyToReadOnlyDirectory());
                taskList.Add(new WriteManifestFile());
            }

            //调用SBP的构建管线
            ReturnCode returnCode = ContentPipeline.BuildAssetBundles(buildParam, content,
                out IBundleBuildResults result, taskList, buildInfoParam,configParam);

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
                OutputFolder = outputFolder,
                ReturnCode = returnCode,
                Result = result,
            };
            OnBundleBuildPostProcess(postData);
            
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
        private static string CreateFullOutputFolder(BundleBuildConfigSO bundleBuildConfig, BuildTarget targetPlatform)
        {
            string fullOutputFolder = EditorUtil.GetFullOutputFolder(bundleBuildConfig.OutputRootDirectory, targetPlatform,
                bundleBuildConfig.ManifestVersion);
            EditorUtil.CreateEmptyDirectory(fullOutputFolder);
            return fullOutputFolder;
        }

        /// <summary>
        /// 获取SBP用到的构建参数
        /// </summary>
        private static BundleBuildParameters GetSBPParameters(BundleBuildConfigSO bundleBuildConfig,
            BuildTarget targetPlatform, string outputFolder)
        {
            BuildTargetGroup group = UnityEditor.BuildPipeline.GetBuildTargetGroup(targetPlatform);

            BundleBuildParameters parameters = new BundleBuildParameters(targetPlatform, group, outputFolder);

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
            //buildTasks.Add(new GenerateSubAssetPathMaps());
            buildTasks.Add(new MyGenerateSubAssetPathMaps());
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
