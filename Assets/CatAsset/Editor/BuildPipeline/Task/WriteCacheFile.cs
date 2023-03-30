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
    /// 写入缓存文件
    /// </summary>
    public class WriteCacheFile : IBuildTask
    {
        [InjectContext(ContextUsage.In)]
        private IBundleBuildConfigParam configParam;
        
        [InjectContext(ContextUsage.In)]
        private IManifestParam manifestParam;

        [InjectContext(ContextUsage.In)] 
        private IBundleBuildParameters buildParam;
        
        public int Version { get; }
        
        public ReturnCode Run()
        {
            //复制资源包构建输出结果到缓存文件夹中
            string folder = EditorUtil.GetBundleCacheFolder(configParam.Config.OutputRootDirectory,
                configParam.TargetPlatform);
            EditorUtil.CreateEmptyDirectory(folder);
            EditorUtil.CopyDirectory(((BundleBuildParameters)buildParam).OutputFolder,folder);
            
            //写入资源缓存清单
            folder = EditorUtil.GetAssetCacheManifestFolder(configParam.Config.OutputRootDirectory);
            EditorUtil.CreateEmptyDirectory(folder);
            AssetCacheManifest assetCacheManifest = new AssetCacheManifest();
            foreach (BundleManifestInfo bundleManifestInfo in manifestParam.Manifest.Bundles)
            {
                foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
                {
                    string md5 = RuntimeUtil.GetFileMD5(assetManifestInfo.Name);
                    assetCacheManifest.Caches.Add(new AssetCacheManifest.AssetCache()
                    {
                        Name = assetManifestInfo.Name,
                        MD5 =  md5,
                    });
                }
            }
            string json = EditorJsonUtility.ToJson(assetCacheManifest,true);
            string path = RuntimeUtil.GetRegularPath(Path.Combine(folder, AssetCacheManifest.ManifestJsonFileName));
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(json);
            }
            
            return ReturnCode.Success;
        }
    }
}