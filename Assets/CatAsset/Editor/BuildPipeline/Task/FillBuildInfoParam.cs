using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                //读取上次完整构建时的资源包信息
                string folder = EditorUtil.GetBundleCacheFolder(config.OutputRootDirectory, configParam.TargetPlatform);
                string path = RuntimeUtil.GetRegularPath(Path.Combine(folder, CatAssetManifest.ManifestJsonFileName));
                CatAssetManifest cachedManifest = CatAssetManifest.DeserializeFromJson(File.ReadAllText(path));
                HashSet<string> cachedBundles = new HashSet<string>();
                foreach (var bundleManifestInfo in cachedManifest.Bundles)
                {
                    cachedBundles.Add(bundleManifestInfo.BundleIdentifyName);
                }
                
                //读取上次完整构建时的资源文件MD5信息
                folder = EditorUtil.GetAssetCacheManifestFolder(config.OutputRootDirectory);
                path = RuntimeUtil.GetRegularPath(Path.Combine(folder, AssetCacheManifest.ManifestJsonFileName));
                string json = File.ReadAllText(path);
                AssetCacheManifest assetCacheManifest = JsonUtility.FromJson<AssetCacheManifest>(json);
                Dictionary<string, string> cacheMD5Dict = assetCacheManifest.GetCacheDict();

                //深拷贝一份构建配置进行操作
                BundleBuildConfigSO clonedConfig = Object.Instantiate(config);  

                //计算补丁包前将要构建的所有非原生资源的字典
                Dictionary<string, AssetBuildInfo> allAssetDict = new Dictionary<string, AssetBuildInfo>();
                foreach (BundleBuildInfo bundleBuildInfo in clonedConfig.Bundles)
                {
                    if (bundleBuildInfo.IsRaw)
                    {
                        continue;
                    }
                    foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                    {
                        allAssetDict.Add(assetBuildInfo.Name,assetBuildInfo);
                    }
                }

                //补丁包
                HashSet<BundleBuildInfo> patchBundles = new HashSet<BundleBuildInfo>();
                //计算补丁包
                for (int i = clonedConfig.Bundles.Count - 1; i >= 0; i--)
                {
                    BundleBuildInfo bundleBuildInfo = clonedConfig.Bundles[i];

                    //是新资源包 直接跳过处理
                    if (!cachedBundles.Contains(bundleBuildInfo.BundleIdentifyName))
                    {
                        continue;
                    }
                    
                    //是旧资源包
                    //那么如果有 新资源 或者 旧资源文件发生了变化 就认为是补丁包
                    for (int j = bundleBuildInfo.Assets.Count - 1; j >= 0; j--)
                    {
                        AssetBuildInfo assetBuildInfo = bundleBuildInfo.Assets[j];
                        
                        
                        bool isNewOrChangedAsset = false;
                        if (!cacheMD5Dict.TryGetValue(assetBuildInfo.Name,out string cachedMD5))
                        {
                            //新资源
                            isNewOrChangedAsset = true;
                            Debug.Log($"新增了资源:{assetBuildInfo.Name}");
                        }
                        else
                        {
                            string md5 = RuntimeUtil.GetFileMD5(assetBuildInfo.Name);
                            if (md5 != cachedMD5)
                            {
                                //旧资源文件发生了变化
                                isNewOrChangedAsset = true;
                                Debug.Log($"{assetBuildInfo.Name}发生了变化:{cachedMD5} -> {md5}");
                            }
                        }
                        
                        if (isNewOrChangedAsset)
                        {
                            //是补丁包
                            patchBundles.Add(bundleBuildInfo);
                        }
                        else
                        {
                            //源文件没有发生变化 从所属资源包的资源列表中删除
                            bundleBuildInfo.Assets.RemoveAt(j);
                        }
                    }

                    if (!patchBundles.Contains(bundleBuildInfo))
                    {
                        //是旧资源包 但没有资源变化或新增资源 不是补丁包 需要移除
                        clonedConfig.Bundles.RemoveAt(i);
                    }
                }
                
                
                //计算完补丁包后 将要构建的所有非原生资源的字典
                Dictionary<string, AssetBuildInfo> patchAssetDict = new Dictionary<string, AssetBuildInfo>();
                foreach (BundleBuildInfo bundleBuildInfo in clonedConfig.Bundles)
                {
                    if (bundleBuildInfo.IsRaw)
                    {
                        continue;
                    }
                    foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                    {
                        patchAssetDict.Add(assetBuildInfo.Name,assetBuildInfo);
                    }
                }
                
                //重命名补丁包
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
                
                //处理补丁包资源依赖导致的冗余资源
                //将冗余资源单独构建到这个一个临时资源包中 但并不使用此资源包
                BundleBuildInfo redundancyDepBundle = new BundleBuildInfo(string.Empty, EditorUtil.RedundancyDepBundleName,
                    GroupInfo.DefaultGroup, false, BundleCompressOptions.LZ4, BundleEncryptOptions.NotEncrypt);
                HashSet<AssetBuildInfo> redundancyDepAssets = new HashSet<AssetBuildInfo>();

                foreach (var pair in patchAssetDict)
                {
                    AssetBuildInfo patchAsset = pair.Value;
                    List<string> deps = EditorUtil.GetDependencies(patchAsset.Name);
                    foreach (string dep in deps)
                    {
                        if (!patchAssetDict.TryGetValue(dep,out var value))
                        {
                            //未出现在此次补丁包构建中的依赖资源 是冗余资源
                            var depAssetBuildInfo = allAssetDict[dep];
                            redundancyDepAssets.Add(depAssetBuildInfo);
                            Debug.Log($"剔除冗余的依赖资源:{dep}");
                        }
                    }
                }
               
                if (redundancyDepAssets.Count > 0)
                {
                    redundancyDepBundle.Assets.AddRange(redundancyDepAssets.ToList());
                    clonedConfig.Bundles.Add(redundancyDepBundle);
                }
                
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