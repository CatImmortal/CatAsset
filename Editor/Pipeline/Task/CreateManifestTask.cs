using System;
using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public class CreateManifestTask : IBuildPipelineTask
    {
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private BundleBuildConfigParam bundleBuildConfigParam;

        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private BundleBuildsParam bundleBuildsParam;

        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private FullOutputDirectoryParam fullOutputDirectoryParam;

        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private UnityManifestParam unityManifestParam;

        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.Out)]
        private CatAssetManifestParam catAssetManifestParam;

        public TaskResult Run()
        {
            try
            {
                BundleBuildConfigSO bundleBuildConfig =
                    bundleBuildConfigParam.Config;

                List<BundleBuildInfo> normalBundleBuilds =
                    bundleBuildsParam.NormalBundleBuilds;

                List<BundleBuildInfo> rawBundleBuilds =
                    bundleBuildsParam.RawBundleBuilds;

                string directory = fullOutputDirectoryParam.FullOutputDirectory;

                AssetBundleManifest unityManifest =
                    unityManifestParam.UnityManifest;

                //创建资源清单
                CatAssetManifest manifest = new CatAssetManifest
                {
                    GameVersion = Application.version,
                    ManifestVersion = bundleBuildConfig.ManifestVersion,
                    Bundles = new List<BundleManifestInfo>(),
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

                    string fullPath = Path.Combine(directory, bundleBuildInfo.RelativePath);
                    FileInfo fi = new FileInfo(fullPath);
                    bundleManifestInfo.Length = fi.Length;

                    bundleManifestInfo.Hash = unityManifest.GetAssetBundleHash(bundleBuildInfo.RelativePath);

                    foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                    {
                        AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                        {
                            AssetName = assetBuildInfo.AssetName,
                            AssetType = AssetDatabase.GetMainAssetTypeAtPath(assetBuildInfo.AssetName),
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

                    string fullPath = Path.Combine(directory, bundleBuildInfo.RelativePath);
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
                catAssetManifestParam = new CatAssetManifestParam()
                {
                    Manifest = manifest
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