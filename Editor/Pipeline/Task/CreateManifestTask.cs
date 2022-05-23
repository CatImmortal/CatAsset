using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset.Editor.Task
{
    public class CreateManifestTask : IBuildPipelineTask
    {
        public TaskResult Run()
        {
            try
            {

                BundleBuildConfigSO bundleBuildConfig =
                    BuildPipelineRunner.GetPipelineParam<BundleBuildConfigSO>(nameof(BundleBuildConfigSO));

                List<BundleBuildInfo> normalBundleBuilds =
                    BuildPipelineRunner.GetPipelineParam<List<BundleBuildInfo>>(BuildPipeline.NormalBundleBuilds);

                List<BundleBuildInfo> rawBundleBuilds =
                    BuildPipelineRunner.GetPipelineParam<List<BundleBuildInfo>>(BuildPipeline.RawBundleBuilds);
                string fullOutputPath = BuildPipelineRunner.GetPipelineParam<string>(BuildPipeline.FullOutputPath);

                AssetBundleManifest unityManifest =
                    BuildPipelineRunner.GetPipelineParam<AssetBundleManifest>(nameof(AssetBundleManifest));

                //创建资源清单
                CatAssetManifest manifest = new CatAssetManifest
                {
                    GameVersion = Application.version,
                    ManifestVersion = bundleBuildConfig.ManifestVersion,
                };

                //创建普通资源包的清单信息
                foreach (BundleBuildInfo bundleBuildInfo in normalBundleBuilds)
                {
                    BundleManifestInfo bundleManifestInfo = new BundleManifestInfo()
                    {
                        RelativePath = bundleBuildInfo.RelativePath,
                        Directory = bundleBuildInfo.DirectoryName,
                        BundleName = bundleBuildInfo.BundleName,
                        Group = bundleBuildInfo.Group,
                        IsRaw = false,
                    };
                    manifest.Bundles.Add(bundleManifestInfo);

                    bundleManifestInfo.IsScene = bundleBuildInfo.Assets[0].AssetName.EndsWith(".unity");

                    string fullPath = Path.Combine(fullOutputPath, bundleBuildInfo.RelativePath);
                    FileInfo fi = new FileInfo(fullPath);
                    bundleManifestInfo.Length = fi.Length;

                    bundleManifestInfo.Hash = unityManifest.GetAssetBundleHash(bundleBuildInfo.RelativePath);

                    foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                    {
                        AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                        {
                            AssetName = assetBuildInfo.AssetName,
                        };
                        bundleManifestInfo.Assets.Add(assetManifestInfo);

                        //依赖列表不进行递归记录 因为加载的时候会对依赖进行递归加载
                        assetManifestInfo.Dependencies = Util.GetDependencies(assetManifestInfo.AssetName, false);
                    }
                }

                //创建原生资源包的清单信息
                foreach (BundleBuildInfo bundleBuildInfo in rawBundleBuilds)
                {
                    BundleManifestInfo bundleManifestInfo = new BundleManifestInfo()
                    {
                        RelativePath = bundleBuildInfo.RelativePath,
                        Directory = bundleBuildInfo.DirectoryName,
                        BundleName = bundleBuildInfo.BundleName,
                        Group = bundleBuildInfo.Group,
                        IsRaw = true,
                        IsScene = false,
                    };
                    manifest.Bundles.Add(bundleManifestInfo);

                    string fullPath = Path.Combine(fullOutputPath, bundleBuildInfo.RelativePath);
                    byte[] bytes = File.ReadAllBytes(fullPath);
                    bundleManifestInfo.Length = bytes.Length;

                    bundleManifestInfo.Hash = Hash128.Compute(bytes);

                    AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                    {
                        AssetName = bundleBuildInfo.Assets[0].AssetName,
                    };
                    bundleManifestInfo.Assets.Add(assetManifestInfo);
                }

                manifest.Bundles.Sort();
                
                BuildPipelineRunner.InjectPipelineParam(nameof(CatAssetManifest),manifest);
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