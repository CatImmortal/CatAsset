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
                        configParam.Config.GlobalCompress);
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
                };
                manifest.Bundles.Add(bundleManifestInfo);

                if (bundleBuildInfo.Assets.Count > 0)
                {
                    //是场景资源包
                    bundleManifestInfo.IsScene = bundleBuildInfo.Assets[0].Name.EndsWith(".unity");
                }

                string fullPath = Path.Combine(outputFolder, bundleBuildInfo.UniqueBundleName);
                FileInfo fi = new FileInfo(fullPath);
                bundleManifestInfo.Length = (ulong)fi.Length;
                bundleManifestInfo.MD5 = RuntimeUtil.GetFileMD5(fullPath);
                BundleDetails details = results.BundleInfos[bundleManifestInfo.UniqueBundleName];
                if (configParam.TargetPlatform == BuildTarget.WebGL)
                {
                    //WebGL平台 记录Hash128用于缓存系统
                    bundleManifestInfo.Hash = details.Hash.ToString();
                }
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
                    };

                    bundleManifestInfo.Assets.Add(assetManifestInfo);

                    //依赖列表不需要进行递归记录 因为加载的时候会对依赖进行递归加载
                    assetManifestInfo.Dependencies = EditorUtil.GetDependencies(assetManifestInfo.Name, false);
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
                };
                manifest.Bundles.Add(bundleManifestInfo);

                string fullPath = Path.Combine(outputFolder, bundleBuildInfo.UniqueBundleName);
                FileInfo fi = new FileInfo(fullPath);
                bundleManifestInfo.Length = (ulong)fi.Length;
                bundleManifestInfo.MD5 = RuntimeUtil.GetFileMD5(fullPath);
                
                //资源信息
                AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                {
                    Name = bundleBuildInfo.Assets[0].Name,
                };
                bundleManifestInfo.Assets.Add(assetManifestInfo);
            }

            manifest.Bundles.Sort();

            manifestParam = new ManifestParam(manifest,outputFolder);
            
            return ReturnCode.Success;
        }
    }
}