using System;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor.Task
{
    /// <summary>
    /// 创建资源包构建输出目录的任务
    /// </summary>
    public class CreateOutputDirectoryTask : IBuildPipelineTask
    {

        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                BundleBuildConfigSO bundleBuildConfig =
                    BuildPipelineRunner.GetPipelineParam<BundleBuildConfigSO>(nameof(BundleBuildConfigSO));
            
                BuildTarget targetPlatform = BuildPipelineRunner.GetPipelineParam<BuildTarget>(nameof(BuildTarget));
            
                //创建完整资源包构建输出目录
                string fullOutputPath = BuildPipeline.GetFullOutputPath(bundleBuildConfig.OutputPath, targetPlatform,
                    bundleBuildConfig.ManifestVersion);
                Util.CreateEmptyDirectory(fullOutputPath);
                
                BuildPipelineRunner.InjectPipelineParam(BuildPipeline.FullOutputPath ,fullOutputPath);
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