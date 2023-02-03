using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatAsset.Runtime;
using UnityEditor;
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


            HashSet<string> atlasPackableSet = new HashSet<string>();
            if (infoParam.NormalBundleBuilds.Count > 0)
            {
                //非仅构建原生资源包 找出所有图集散图
                var guids = AssetDatabase.FindAssets("t:SpriteAtlas");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    List<string> sprites = EditorUtil.GetDependencies(path, false);
                    foreach (string sprite in sprites)
                    {
                        atlasPackableSet.Add(sprite);
                    }
                }
            }


            //创建资源清单
            CatAssetManifest manifest = new CatAssetManifest
            {
                GameVersion = Application.version,
                ManifestVersion = configParam.Config.ManifestVersion,
                Platform = configParam.TargetPlatform.ToString(),
                Bundles = new List<BundleManifestInfo>(),
            };

            //增加内置Shader资源包的构建信息
            string builtInShadersBundleName = "UnityBuiltInShaders.bundle";
            if (results.BundleInfos.ContainsKey(builtInShadersBundleName))
            {
                BundleBuildInfo bundleBuildInfo =
                    new BundleBuildInfo(string.Empty, builtInShadersBundleName, GroupInfo.DefaultGroup, false,
                        configParam.Config.GlobalCompress,configParam.Config.GlobalEncrypt);
                infoParam.NormalBundleBuilds.Add(bundleBuildInfo);
            }

            //创建普通资源包的清单信息
            foreach (BundleBuildInfo bundleBuildInfo in infoParam.NormalBundleBuilds)
            {
                BundleManifestInfo bundleManifestInfo = new BundleManifestInfo()
                {
                    Directory = bundleBuildInfo.DirectoryName,
                    BundleName = bundleBuildInfo.BundleName,
                    Group = bundleBuildInfo.Group,
                    IsRaw = false,
                    EncryptOption = bundleBuildInfo.EncryptOption,
                    Assets = new List<AssetManifestInfo>(),
                };
                manifest.Bundles.Add(bundleManifestInfo);

                if (bundleBuildInfo.Assets.Count > 0)
                {
                    //是场景资源包
                    bundleManifestInfo.IsScene = bundleBuildInfo.Assets[0].Name.EndsWith(".unity");
                }
                BundleDetails details = results.BundleInfos[bundleManifestInfo.BundleIdentifyName];
                if (details.Dependencies.Contains(builtInShadersBundleName))
                {
                    //依赖内置Shader资源包
                    bundleManifestInfo.IsDependencyBuiltInShaderBundle = true;
                }

                //资源信息
                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                    {
                        Name = assetBuildInfo.Name,
                        IsAtlasPackable = atlasPackableSet.Contains(assetBuildInfo.Name),
                        Dependencies = EditorUtil.GetDependencies(assetBuildInfo.Name,false),
                    };
                    
                    bundleManifestInfo.Assets.Add(assetManifestInfo);
                }
            }

            //创建原生资源包的清单信息
            foreach (BundleBuildInfo bundleBuildInfo in infoParam.RawBundleBuilds)
            {
                BundleManifestInfo bundleManifestInfo = new BundleManifestInfo()
                {
                    Directory = bundleBuildInfo.DirectoryName,
                    BundleName = bundleBuildInfo.BundleName,
                    Group = bundleBuildInfo.Group,
                    IsRaw = true,
                    IsScene = false,
                    EncryptOption = bundleBuildInfo.EncryptOption,
                    Assets = new List<AssetManifestInfo>(),
                };
                manifest.Bundles.Add(bundleManifestInfo);
                
                //资源信息
                AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                {
                    Name = bundleBuildInfo.Assets[0].Name,
                };
                
                bundleManifestInfo.Assets.Add(assetManifestInfo);
            }

            manifestParam = new ManifestParam(manifest,outputFolder);

            return ReturnCode.Success;
        }
    }
}
