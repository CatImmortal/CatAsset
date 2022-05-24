using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 构建AssetBundle的任务
    /// </summary>
    public class BuildAssetBundleTask : IBuildPipelineTask
    {
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private BundleBuildConfigParam bundleBuildConfigParam;

        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private FullOutputDirectoryParam fullOutputDirectoryParam;

        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private BundleBuildsParam bundleBuildsParam;

        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.Out)]
        private UnityManifestParam unityManifestParam;

        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                BundleBuildConfigSO bundleBuildConfig = bundleBuildConfigParam.Config;

                BuildTarget targetPlatform = bundleBuildConfigParam.TargetPlatform;

                List<AssetBundleBuild> bundleBuilds = bundleBuildsParam.AssetBundleBuilds;

                string directory = fullOutputDirectoryParam.FullOutputDirectory;

                if (bundleBuilds == null)
                {
                    return TaskResult.Success;
                }

                //构建AssetBundle
                AssetBundleManifest unityManifest =
                    UnityEditor.BuildPipeline.BuildAssetBundles(directory,
                        bundleBuilds.ToArray(), bundleBuildConfig.Options,
                        targetPlatform);

                unityManifestParam = new UnityManifestParam()
                {
                    UnityManifest = unityManifest
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