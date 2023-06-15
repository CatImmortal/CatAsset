using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 补丁资源计算辅助类
    /// </summary>
    public class PatchAssetCalculateHelper
    {
        //双向依赖记录
        private Dictionary<string, List<string>> upStreamDict = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> downStreamDict = new Dictionary<string, List<string>>();
        
        //资源名 -> 本次构建时所属的资源包名
        private Dictionary<string, string> assetToBundle = new Dictionary<string, string>();

        //资源名 -> 上次完整构建时所属的资源包名
        private Dictionary<string, string> cacheAssetToBundle = new Dictionary<string, string>();
                
        //读取上次完整构建时的资源缓存清单
        private AssetCacheManifest assetCacheManifest;
                
        //资源名 -> 上次完整构建时的资源缓存信息
        private Dictionary<string, AssetCacheManifest.AssetCacheInfo> assetCacheDict;

        //资源名 -> 当前资源缓存信息
        private Dictionary<string, AssetCacheManifest.AssetCacheInfo> curAssetCacheDict =
            new Dictionary<string, AssetCacheManifest.AssetCacheInfo>();

        //资源名 -> 是否已变化
        private Dictionary<string, bool> assetChangeStateDict = new Dictionary<string, bool>();
        
        //资源名 -> 是否为补丁资源
        private Dictionary<string, bool> assetPatchStateDict = new Dictionary<string, bool>();


        
        public BundleBuildConfigSO Calculate(BundleBuildConfigSO config, BuildTarget buildTarget)
        {
            assetCacheManifest = ReadAssetCache(config);
            assetCacheDict = assetCacheManifest.GetCacheDict();
            
            //深拷贝一份构建配置进行操作
            BundleBuildConfigSO clonedConfig = Object.Instantiate(config);
            
            //获取双向依赖
            GetDependencyChain(config);
                
            //读取上次完整构建时的资源包信息
            ReadCachedManifest(config,buildTarget);
                
            //计算补丁资源
            CalPatchAsset(config, clonedConfig);

            return clonedConfig;
        }
        
        /// <summary>
        /// 读取上次完整构建时的资源缓存清单
        /// </summary>
        private static AssetCacheManifest ReadAssetCache(BundleBuildConfigSO config)
        {
            string folder = EditorUtil.GetAssetCacheManifestFolder(config.OutputRootDirectory);
            string path = RuntimeUtil.GetRegularPath(Path.Combine(folder, AssetCacheManifest.ManifestJsonFileName));
            string json = File.ReadAllText(path);
            AssetCacheManifest assetCacheManifest = JsonUtility.FromJson<AssetCacheManifest>(json);
            return assetCacheManifest;
        }
        
        /// <summary>
        /// 获取双向依赖
        /// </summary>
        private void GetDependencyChain(BundleBuildConfigSO config)
        {
            int index = 0;
            foreach (var bundle in config.Bundles)
            {
                foreach (var asset in bundle.Assets)
                {
                    index++;
                    EditorUtility.DisplayProgressBar($"获取依赖信息", $"{asset.Name}", index / (config.AssetCount * 1.0f));
                    
                    //上游依赖
                    var deps = EditorUtil.GetDependencies(asset.Name);
                    upStreamDict.Add(asset.Name, deps);

                    //下游依赖
                    foreach (string dep in deps)
                    {
                        if (!downStreamDict.TryGetValue(dep, out List<string> list))
                        {
                            list = new List<string>();
                            downStreamDict.Add(dep, list);
                        }

                        list.Add(asset.Name);
                    }

                    assetToBundle.Add(asset.Name, bundle.BundleIdentifyName);
                }
            }
            
            EditorUtility.ClearProgressBar();
        }
        
        /// <summary>
        /// 读取上次完整构建时的资源包信息
        /// </summary>
        private void ReadCachedManifest(BundleBuildConfigSO config,BuildTarget buildTarget)
        {
            string folder = EditorUtil.GetBundleCacheFolder(config.OutputRootDirectory, buildTarget);
            string path = RuntimeUtil.GetRegularPath(Path.Combine(folder, CatAssetManifest.ManifestJsonFileName));
            CatAssetManifest cachedManifest = CatAssetManifest.DeserializeFromJson(File.ReadAllText(path));
            foreach (var bundle in cachedManifest.Bundles)
            {
                foreach (var asset in bundle.Assets)
                {
                    cacheAssetToBundle.Add(asset.Name, bundle.BundleIdentifyName);
                }
            }
        }
        
        /// <summary>
        /// 计算补丁资源
        /// </summary>
        private void CalPatchAsset(BundleBuildConfigSO config, BundleBuildConfigSO clonedConfig)
        {
            int index = 0;
            for (int i = clonedConfig.Bundles.Count - 1; i >= 0; i--)
            {
                var bundle = clonedConfig.Bundles[i];

                //此资源包是否全部资源都是补丁资源
                bool isAllPatch = true;

                for (int j = bundle.Assets.Count - 1; j >= 0; j--)
                {
                    var asset = bundle.Assets[j];
                    index++;
                    EditorUtility.DisplayProgressBar($"计算补丁资源", $"{asset.Name}", index / (config.AssetCount * 1.0f));
                    
                    bool isPatch = IsPatchAsset(asset.Name);
                    
                    if (isPatch)
                    {
                        Debug.Log($"发现补丁资源:{asset.Name}");
                    }
                    else
                    {
                        //不是补丁资源 移除掉
                        bundle.Assets.RemoveAt(j);
                        isAllPatch = false;
                    }
                }

                if (bundle.Assets.Count > 0)
                {
                    //是补丁包
                    
                    if (!isAllPatch)
                    {
                        //有部分资源不是补丁资源 需要改名 否则直接用正式包的名字了
                        var part = bundle.BundleName.Split('.');
                        bundle.BundleName = $"{part[0]}_patch.{part[1]}";
                        bundle.BundleIdentifyName =
                            BundleBuildInfo.GetBundleIdentifyName(bundle.DirectoryName, bundle.BundleName);
                    }
                }
                else
                {
                    //不是补丁包 需要移除
                    clonedConfig.Bundles.RemoveAt(i);
                }
            }

            EditorUtility.ClearProgressBar();
        }
        
        /// <summary>
        /// 是否为补丁资源
        /// </summary>
        private bool IsPatchAsset(string assetName)
        {
            //0.已经计算过状态了
            if (assetPatchStateDict.TryGetValue(assetName, out bool isPatch))
            {
                return isPatch;
            }
            
            //1.自身是否已变化
            isPatch = IsChangedAsset(assetName);
            if (isPatch)
            {
                assetPatchStateDict.Add(assetName,true);
                return true;
            }
            
            //2.此资源依赖的上游资源是否为补丁资源
            //补丁资源会传染给依赖链下游的所有资源
            if (upStreamDict.TryGetValue(assetName, out var upStreamList))
            {
                foreach (string upStream in upStreamList)
                {
                    isPatch = IsPatchAsset(upStream);
                    if (isPatch)
                    {
                        assetPatchStateDict.Add(assetName,true);
                        return true;
                    }
                }
            }
            
            //补丁资源不传染给依赖链上游资源
            //而是通过隐式依赖自动包含机制 故意冗余一份 使得补丁资源的依赖和它本身在一个资源包内
            //以防止正式包的资源 依赖到 补丁包依赖的资源 时 丢失依赖
            //这样就会在正式包和补丁包里各包含一份相同的依赖资源 保证正式包依赖不丢失
            
            //假设有D -> C -> B ->A 和 D -> E 的依赖链，且C为变化的资源
            //那么最终会将C以及依赖C的B和A作为补丁资源，D作为C的隐式依赖包含进C的补丁包里
            //运行时 E依赖的D 和 C依赖的D 会分别在不同的包里 保证E依赖不丢失
            
            assetPatchStateDict.Add(assetName,false);
            return false;
            
        }
        
        /// <summary>
        /// 是否为已变化的资源
        /// </summary>
        private bool IsChangedAsset(string assetName)
        {
            //0.已经计算过状态了
            if (assetChangeStateDict.TryGetValue(assetName, out bool isPatch))
            {
                return isPatch;
            }

            //1.新资源 
            if (!assetCacheDict.TryGetValue(assetName, out var assetCache))
            {
                assetChangeStateDict.Add(assetName, true);
                return true;
            }

            //2.变化的旧资源
            if (!curAssetCacheDict.TryGetValue(assetName, out var curAssetCache))
            {
                curAssetCache = AssetCacheManifest.AssetCacheInfo.Create(assetName);
                curAssetCacheDict.Add(assetName, curAssetCache);
            }

            if (curAssetCache != assetCache)
            {
                assetChangeStateDict.Add(assetName, true);
                return true;
            }

            //3.被移动到新包的旧资源
            if (assetToBundle[assetName] != cacheAssetToBundle[assetName])
            {
                assetChangeStateDict.Add(assetName, true);
                return true;
            }

            //未变化
            assetChangeStateDict.Add(assetName, false);
            return false;
        }

    }
}