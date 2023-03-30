using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 填充资源包构建信息参数
    /// </summary>
    public class FillBuildInfoParam : IBuildTask
    {
        public int Version { get; }
        
        
        [InjectContext(ContextUsage.In)]
        private IBundleBuildParameters buildParam;

        [InjectContext(ContextUsage.InOut)]
        private IBundleBuildInfoParam buildInfoParam;
        
        [InjectContext(ContextUsage.In)]
        private IBundleBuildConfigParam configParam;

        [InjectContext(ContextUsage.In)]
        private IBuildCache buildCache;
        
        [InjectContext(ContextUsage.InOut)]
        private IBundleBuildContent content;

        [InjectContext(ContextUsage.InOut)]
        private IBuildContent content2;
        
        public ReturnCode Run()
        {
            BundleBuildConfigSO config = configParam.Config;
            if (!configParam.IsBuildPatch)
            {
                //构建完整资源包
                buildInfoParam = new BundleBuildInfoParam(config.GetAssetBundleBuilds(),config.GetNormalBundleBuilds(),
                    config.GetRawBundleBuilds());
            }
            else
            {
                string folder = EditorUtil.GetBundleCacheFolder(config.OutputRootDirectory, configParam.TargetPlatform);
                string path = RuntimeUtil.GetRegularPath(Path.Combine(folder, CatAssetManifest.ManifestJsonFileName));
                CatAssetManifest cachedManifest = CatAssetManifest.DeserializeFromJson(File.ReadAllText(path));

                HashSet<string> cachedBundles = new HashSet<string>();
                foreach (var bundleManifestInfo in cachedManifest.Bundles)
                {
                    cachedBundles.Add(bundleManifestInfo.BundleIdentifyName);
                }
                
                //构建补丁资源包
                folder = EditorUtil.GetAssetCacheManifestFolder(config.OutputRootDirectory);
                path = RuntimeUtil.GetRegularPath(Path.Combine(folder, AssetCacheManifest.ManifestJsonFileName));
                string json = File.ReadAllText(path);
                AssetCacheManifest assetCacheManifest = JsonUtility.FromJson<AssetCacheManifest>(json);
                Dictionary<string, string> cacheDict = assetCacheManifest.GetCacheDict();

                var clonedConfig = Object.Instantiate(config);  //深拷贝一份进行操作
                HashSet<BundleBuildInfo> patchBundles = new HashSet<BundleBuildInfo>();
                
                for (int i = clonedConfig.Bundles.Count - 1; i >= 0; i--)
                {
                    BundleBuildInfo bundleBuildInfo = clonedConfig.Bundles[i];
                    
                    //是新资源包 直接跳过处理
                    if (!cachedBundles.Contains(bundleBuildInfo.BundleIdentifyName))
                    {
                        continue;
                    }
                    
                    //是旧资源包
                    //有新资源 或者 旧资源文件发生了变化 就认为是补丁包
                    for (int j = bundleBuildInfo.Assets.Count - 1; j >= 0; j--)
                    {
                        AssetBuildInfo assetBuildInfo = bundleBuildInfo.Assets[j];

                        bool isNewOrChangedAsset = false;
                        if (!cacheDict.TryGetValue(assetBuildInfo.Name,out string cachedMD5))
                        {
                            //新资源
                            isNewOrChangedAsset = true;
                        }
                        else
                        {
                            string md5 = RuntimeUtil.GetFileMD5(assetBuildInfo.Name);
                            if (md5 != cachedMD5)
                            {
                                //旧资源文件发生了变化
                                Debug.Log($"{assetBuildInfo.Name}发生了变化:{cachedMD5} -> {md5}");
                                isNewOrChangedAsset = true;
                            }
                        }
                        
                        if (isNewOrChangedAsset)
                        {
                            //是补丁包
                            patchBundles.Add(bundleBuildInfo);
                        }
                        else
                        {
                            //无变化的资源文件 从 bundleBuildInfo 中删除
                            bundleBuildInfo.Assets.RemoveAt(j);
                        }
                    }

                    if (!patchBundles.Contains(bundleBuildInfo))
                    {
                        //是旧资源包 但不是补丁包 需要移除
                        clonedConfig.Bundles.RemoveAt(i);
                    }
                }

                foreach (BundleBuildInfo patchBundle in patchBundles)
                {
                    if (patchBundle.IsRaw)
                    {
                        //原生资源包就不当补丁包处理了 直接构建为新资源包
                        continue;
                    }
                    
                    //将资源包名设置为对应补丁包的名字
                    var part = patchBundle.BundleName.Split('.');
                    patchBundle.BundleName = $"{part[0]}_patch.{part[1]}";
                    patchBundle.BundleIdentifyName =
                        BundleBuildInfo.GetBundleIdentifyName(patchBundle.DirectoryName, patchBundle.BundleName);
                }
                
                //TODO:处理冗余资源
                
                buildInfoParam = new BundleBuildInfoParam(clonedConfig.GetAssetBundleBuilds(),clonedConfig.GetNormalBundleBuilds(),
                    clonedConfig.GetRawBundleBuilds());
                
            }

            ((BundleBuildParameters)buildParam).SetBundleBuilds(buildInfoParam.NormalBundleBuilds);
            content = new BundleBuildContent(buildInfoParam.AssetBundleBuilds);
            content2 = content;
            
            return ReturnCode.Success;
        }
    }
}