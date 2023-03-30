using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 合并补丁包资源清单
    /// </summary>
    public class MergePatchManifest : IBuildTask
    {
        [InjectContext(ContextUsage.In)]
        private IBundleBuildParameters buildParam;

        [InjectContext(ContextUsage.In)]
        private IBundleBuildConfigParam configParam;

        [InjectContext(ContextUsage.InOut)]
        private IManifestParam manifestParam;

        /// <inheritdoc />
        public int Version => 1;

        /// <inheritdoc />
        public ReturnCode Run()
        {
            var config = configParam.Config;
            
            var folder = EditorUtil.GetBundleCacheFolder(config.OutputRootDirectory, configParam.TargetPlatform);

            //本次补丁包构建的资源
            HashSet<string> patchAssets = new HashSet<string>();
            foreach (BundleManifestInfo bundleManifestInfo in manifestParam.Manifest.Bundles)
            {
                foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
                {
                    patchAssets.Add(assetManifestInfo.Name);
                }
            }

            //修改资源清单 移除重复资源
            string path = RuntimeUtil.GetRegularPath(Path.Combine(folder, CatAssetManifest.ManifestJsonFileName));
            CatAssetManifest cachedManifest = CatAssetManifest.DeserializeFromJson(File.ReadAllText(path));
            for (int i = cachedManifest.Bundles.Count - 1; i >= 0; i--)
            {
                BundleManifestInfo bundleManifestInfo = cachedManifest.Bundles[i];
                if (bundleManifestInfo.BundleName == RuntimeUtil.BuiltInShaderBundleName)
                {
                    //跳过内置Shader资源包
                    continue;
                }
                
                for (int j = bundleManifestInfo.Assets.Count - 1; j >= 0; j--)
                {
                    //删掉已经在补丁包中的资源信息
                    AssetManifestInfo assetManifestInfo = bundleManifestInfo.Assets[j];
                    if (patchAssets.Contains(assetManifestInfo.Name))
                    {
                        bundleManifestInfo.Assets.RemoveAt(j);
                    }
                }

                if (bundleManifestInfo.Assets.Count == 0)
                {
                    //删掉所有资源都在补丁包里的资源包
                    cachedManifest.Bundles.RemoveAt(i);
                }
            }

            //合并资源包
            string outputFolder = ((BundleBuildParameters)buildParam).OutputFolder;
            foreach (BundleManifestInfo bundleManifestInfo in cachedManifest.Bundles)
            {
                string sourcePath = Path.Combine(folder, bundleManifestInfo.RelativePath);
                string destPath =
                    RuntimeUtil.GetRegularPath(Path.Combine(outputFolder,bundleManifestInfo.RelativePath));
                File.Copy(sourcePath,destPath);
            }
            
            //合并补丁包资源清单与缓存资源清单
            cachedManifest.Bundles.AddRange(manifestParam.Manifest.Bundles);
            manifestParam = new ManifestParam(cachedManifest, manifestParam.WriteFolder);
            
          
            
            return ReturnCode.Success;
        }


    }
}
