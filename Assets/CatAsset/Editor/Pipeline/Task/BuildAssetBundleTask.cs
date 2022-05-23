using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor.Task
{
    /// <summary>
    /// 构建AssetBundle的任务
    /// </summary>
    public class BuildAssetBundleTask : IBuildPipelineTask
    {
        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                BundleBuildConfigSO bundleBuildConfig =
                    BuildPipelineRunner.GetPipelineParam<BundleBuildConfigSO>(nameof(BundleBuildConfigSO));
            
                BuildTarget targetPlatform = BuildPipelineRunner.GetPipelineParam<BuildTarget>(nameof(BuildTarget));

                string fullOutputPath = BuildPipelineRunner.GetPipelineParam<string>(BuildPipeline.FullOutputPath);
            
                List<AssetBundleBuild> bundleBuilds =
                    BuildPipelineRunner.GetPipelineParam<List<AssetBundleBuild>>(nameof(List<AssetBundleBuild>));

                if (bundleBuilds == null)
                {
                    return TaskResult.Success;
                }

                //构建AssetBundle
                AssetBundleManifest unityManifest =
                    UnityEditor.BuildPipeline.BuildAssetBundles(fullOutputPath, bundleBuilds.ToArray(), bundleBuildConfig.Options,
                        targetPlatform);
            
                BuildPipelineRunner.InjectPipelineParam(nameof(AssetBundleManifest),unityManifest);
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