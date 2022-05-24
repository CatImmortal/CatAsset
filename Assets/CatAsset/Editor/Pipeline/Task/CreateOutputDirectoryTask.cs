using System;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 创建资源包构建输出目录的任务
    /// </summary>
    public class CreateOutputDirectoryTask : IBuildPipelineTask
    {
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private BundleBuildConfigParam bundleBuildConfigParam;

        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.Out)]
        private FullOutputDirectoryParam fullOutputDirectoryParam;

        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                BundleBuildConfigSO bundleBuildConfig = bundleBuildConfigParam.Config;
                BuildTarget targetPlatform = bundleBuildConfigParam.TargetPlatform;

                //创建完整资源包构建输出目录
                string directory = BuildPipeline.GetFullOutputPath(bundleBuildConfig.OutputPath, targetPlatform,
                    bundleBuildConfig.ManifestVersion);
                Util.CreateEmptyDirectory(directory);

                fullOutputDirectoryParam = new FullOutputDirectoryParam()
                {
                    FullOutputDirectory = directory,
                };
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return TaskResult.Failed;
            }

            return TaskResult.Success;
        }
    }
}