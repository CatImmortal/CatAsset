using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset.Editor.Task
{
    /// <summary>
    /// 构建原生资源包的任务
    /// </summary>
    public class BuildRawBundleTask : IBuildPipelineTask
    {
        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                
                string fullOutputPath = BuildPipelineRunner.GetPipelineParam<string>(BuildPipeline.FullOutputPath);
                
                List<BundleBuildInfo> rawBundleBuilds =
                    BuildPipelineRunner.GetPipelineParam<List<BundleBuildInfo>>(BuildPipeline.RawBundleBuilds);
                
                if (rawBundleBuilds == null)
                {
                    return TaskResult.Success;
                }
                
                //遍历原生资源包列表
                foreach (BundleBuildInfo rawBundleBuildInfo in rawBundleBuilds)
                {
                    string rawAssetName = rawBundleBuildInfo.Assets[0].AssetName;
                    string fullDirectory = Path.Combine(fullOutputPath, rawBundleBuildInfo.DirectoryName.ToLower());
                    if (!Directory.Exists(fullDirectory))
                    {
                        Directory.CreateDirectory(fullDirectory);
                    }

                    string targetFileName = Path.Combine(fullOutputPath, rawBundleBuildInfo.RelativePath);
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