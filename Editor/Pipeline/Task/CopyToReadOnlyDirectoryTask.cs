using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace CatAsset.Editor.Task
{
    /// <summary>
    /// 复制指定资源组的资源到只读目录下的任务
    /// </summary>
    public class CopyToReadOnlyDirectoryTask : IBuildPipelineTask
    {
        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                BundleBuildConfigSO bundleBuildConfig =
                    BuildPipelineRunner.GetPipelineParam<BundleBuildConfigSO>(nameof(BundleBuildConfigSO));
                
                string fullOutputPath = BuildPipelineRunner.GetPipelineParam<string>(BuildPipeline.FullOutputPath);

                CatAssetManifest manifest =
                    BuildPipelineRunner.GetPipelineParam<CatAssetManifest>(nameof(CatAssetManifest));
                
                
                if (bundleBuildConfig.IsCopyToReadOnlyPath && bundleBuildConfig.TargetPlatforms.Count == 1)
                {
                    //复制指定资源组的资源到只读目录下
                    
                    //要复制的资源组的Set
                    string copyGroup = bundleBuildConfig.CopyGroup;
                    HashSet<string> copyGroupSet = null;
                    if (!string.IsNullOrEmpty(copyGroup))
                    {
                        copyGroupSet = new HashSet<string>(copyGroup.Split(';'));
                    }

                    Util.CreateEmptyDirectory(Application.streamingAssetsPath);

                    List<BundleManifestInfo> copiedBundles = new List<BundleManifestInfo>();

                    //复制指定组的资源文件
                    foreach (BundleManifestInfo bundleManifestInfo in manifest.Bundles)
                    {
                        if (copyGroupSet != null)
                        {
                            if (!copyGroupSet.Contains(bundleManifestInfo.Group))
                            {
                                //跳过并非指定资源组的资源文件
                                continue;
                            }
                        }

                        
                        FileInfo fi = new FileInfo(Path.Combine(fullOutputPath, bundleManifestInfo.RelativePath));
                        
                        string fullPath = CatAsset.Util.GetReadOnlyPath(bundleManifestInfo.RelativePath);
                        string fullDirectory = CatAsset.Util.GetReadOnlyPath(bundleManifestInfo.Directory.ToLower());
                        if (!Directory.Exists(fullDirectory))
                        {
                            //StreamingAssets下的目录不存在则创建
                            Directory.CreateDirectory(fullDirectory);
                        }

                        fi.CopyTo(fullPath);

                        copiedBundles.Add(bundleManifestInfo);
                    }

                    //根据复制过去的资源包修改资源清单
                    manifest.Bundles = copiedBundles;

                    //写入仅包含被复制的资源包的资源清单文件到只读区下
                    BuildPipelineRunner.InjectPipelineParam(BuildPipeline.FullOutputPath,Application.streamingAssetsPath);
                    BuildPipelineRunner.InjectPipelineParam(nameof(CatAssetManifest),manifest);
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