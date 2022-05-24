using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 构建原生资源包的任务
    /// </summary>
    public class BuildRawBundleTask : IBuildPipelineTask
    {
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private FullOutputDirectoryParam fullOutputDirectoryParam;
        
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private BundleBuildsParam bundleBuildsParam;
        
        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {

                string directory = fullOutputDirectoryParam.FullOutputDirectory;

                List<BundleBuildInfo> rawBundleBuilds = bundleBuildsParam.RawBundleBuilds;
                
                if (rawBundleBuilds == null)
                {
                    return TaskResult.Success;
                }
                
                //遍历原生资源包列表
                foreach (BundleBuildInfo rawBundleBuildInfo in rawBundleBuilds)
                {
                    string rawAssetName = rawBundleBuildInfo.Assets[0].AssetName;
                    string rawBundleDirectory = Path.Combine(directory, rawBundleBuildInfo.DirectoryName.ToLower());
                    if (!Directory.Exists(rawBundleDirectory))
                    {
                        Directory.CreateDirectory(rawBundleDirectory);
                    }

                    string targetFileName = Path.Combine(directory, rawBundleBuildInfo.RelativePath);
                    File.Copy(rawAssetName, targetFileName); //直接将原生资源复制过去
                }
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