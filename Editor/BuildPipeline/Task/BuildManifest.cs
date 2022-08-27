using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using UnityEngine.Build.Pipeline;


namespace CatAsset.Editor
{
    /// <summary>
    /// 构建资源清单
    /// </summary>
    public class BuildManifest : IBuildTask
    {
      
        [InjectContext(ContextUsage.In)] 
        private IBundleBuildParameters buildParam;

        [InjectContext(ContextUsage.In)] 
        private IBundleBuildResults results;
        
        [InjectContext(ContextUsage.In)] 
        private IBundleBuildConfigParam configParam;

        [InjectContext(ContextUsage.In)] 
        private IBundleBuildInfoParam infoParam;

        [InjectContext(ContextUsage.Out)] 
        private IManifestParam manifestParam;
        
        /// <inheritdoc />
        public int Version => 1;

        
        /// <inheritdoc />
        public ReturnCode Run()
        {
            string outputFolder = ((BundleBuildParameters) buildParam).OutputFolder;

            //创建资源清单
            CatAssetManifest manifest = new CatAssetManifest
            {
                GameVersion = Application.version,
                ManifestVersion = configParam.Config.ManifestVersion,
                Bundles = new List<BundleManifestInfo>(),
            };

            //创建普通资源包的清单信息
            foreach (BundleBuildInfo bundleBuildInfo in infoParam.NormalBundleBuilds)
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

                bundleManifestInfo.IsScene = bundleBuildInfo.Assets[0].Name.EndsWith(".unity");

                string fullPath = Path.Combine(outputFolder, bundleBuildInfo.RelativePath);
                FileInfo fi = new FileInfo(fullPath);
                bundleManifestInfo.Length = fi.Length;
                bundleManifestInfo.MD5 = Runtime.Util.GetFileMD5(fullPath);
                
                //资源信息
                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                    {
                        Name = assetBuildInfo.Name,
                        Length = assetBuildInfo.Length,
                    };
                    if (!bundleManifestInfo.IsScene && !bundleManifestInfo.IsRaw)
                    {
                        //非场景和非原生资源才写入资源类型信息
                        assetManifestInfo.Type = assetBuildInfo.Type;
                    }

                    bundleManifestInfo.Assets.Add(assetManifestInfo);

                    //依赖列表不需要进行递归记录 因为加载的时候会对依赖进行递归加载
                    assetManifestInfo.Dependencies = Util.GetDependencies(assetManifestInfo.Name, false);
                }
            }

            //创建原生资源包的清单信息
            foreach (BundleBuildInfo bundleBuildInfo in infoParam.RawBundleBuilds)
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

                string fullPath = Path.Combine(outputFolder, bundleBuildInfo.RelativePath);
                FileInfo fi = new FileInfo(fullPath);
                bundleManifestInfo.Length = fi.Length;
                bundleManifestInfo.MD5 = Runtime.Util.GetFileMD5(fullPath);
                
                //资源信息
                AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                {
                    Name = bundleBuildInfo.Assets[0].Name,
                    Length = bundleManifestInfo.Length,
                };
                bundleManifestInfo.Assets.Add(assetManifestInfo);
            }

            manifest.Bundles.Sort();

            manifestParam = new ManifestParam(manifest,outputFolder);
            
            return ReturnCode.Success;
        }
    }
}