using System;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 合并资源清单与资源包的任务
    /// </summary>
    public class MergeManifestAndBundlesTask : IBuildPipelineTask
    {
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private BundleBuildConfigParam bundleBuildConfigParam;
        
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private FullOutputDirectoryParam fullOutputDirectoryParam;
        
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.InOut)]
        private CatAssetManifestParam catAssetManifestParam;
        
        public TaskResult Run()
        {
            try
            {
                BundleBuildConfigSO bundleBuildConfig = bundleBuildConfigParam.Config;
                BuildTarget targetPlatform = bundleBuildConfigParam.TargetPlatform;
                string directory = fullOutputDirectoryParam.FullOutputDirectory;
                CatAssetManifest rawManifest = catAssetManifestParam.Manifest;
                
                //获取主资源清单(将清单版本号-1)
                string mainOutputPath = Util.GetFullOutputPath(bundleBuildConfig.OutputPath, targetPlatform,
                    bundleBuildConfig.ManifestVersion - 1);
                string mainManifestPath = Path.Combine(mainOutputPath, Util.ManifestFileName);

                //尝试加载主资源清单
                CatAssetManifest mainManifest = null;
                if (File.Exists(mainManifestPath))
                {
                    string json = File.ReadAllText(mainManifestPath);
                    mainManifest = CatJson.JsonParser.ParseJson<CatAssetManifest>(json);
                }
                if (mainManifest == null)
                {
                    return TaskResult.Success;
                }
            
                foreach (BundleManifestInfo bundleManifestInfo in mainManifest.Bundles)
                {
                    if (!bundleManifestInfo.IsRaw)
                    {
                        //合并原生资源包
                        FileInfo fi = new FileInfo(Path.Combine(mainOutputPath, bundleManifestInfo.RelativePath));

                        string fullPath = Path.Combine(directory, bundleManifestInfo.RelativePath);
                        string fullDirectory =  Path.Combine(directory, bundleManifestInfo.Directory.ToLower());
                    
                        if (!Directory.Exists(fullDirectory))
                        {
                            //目录不存在则创建
                            Directory.CreateDirectory(fullDirectory);
                        }

                        fi.CopyTo(fullPath);
                    
                        //合并资源清单记录
                        rawManifest.Bundles.Add(bundleManifestInfo);
                    }
                }

                rawManifest.Bundles.Sort();
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