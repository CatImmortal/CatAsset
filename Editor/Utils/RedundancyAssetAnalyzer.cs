using System.Collections.Generic;

namespace CatAsset.Editor
{
    /// <summary>
    /// 冗余资源分析器
    /// </summary>
    public static class RedundancyAssetAnalyzer
    {
        /// <summary>
        /// 冗余资源分析
        /// </summary>
        public static List<BundleBuildInfo> Analyze(List<BundleBuildInfo> Bundles)
        {
            //获取 资源->所属资源包列表 的字典
            Dictionary<AssetBuildInfo, List<BundleBuildInfo>> assetToBundlesDict = GetAssetToBundlesDict(Bundles);

            // 获取 冗余资源->所属的资源包列表 的字典 与 资源包->包含的冗余资源列表 的字典
            GetRedundancyDict(assetToBundlesDict,
                out Dictionary<AssetBuildInfo, List<BundleBuildInfo>> redundancyBundleDict,
                out Dictionary<BundleBuildInfo, List<AssetBuildInfo>> redundancyAssetDict);

            //生成冗余资源包
            //目标是将有关联的冗余资源分进同一个冗余资源包中
            //没关联的就分进不同的
            List<BundleBuildInfo> redundancyBundles = GenRedundancyBundles(redundancyBundleDict, redundancyAssetDict);

            return redundancyBundles;
        }

        /// <summary>
        /// 获取 资源->所属资源包列表 的字典
        /// </summary>
        private static Dictionary<AssetBuildInfo, List<BundleBuildInfo>> GetAssetToBundlesDict(
            List<BundleBuildInfo> Bundles)
        {
            //资源->所属资源包列表
            Dictionary<AssetBuildInfo, List<BundleBuildInfo>> assetToBundlesDict =
                new Dictionary<AssetBuildInfo, List<BundleBuildInfo>>();

            //统计资源和其所属资源包
            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    if (!assetToBundlesDict.TryGetValue(assetBuildInfo, out List<BundleBuildInfo> set))
                    {
                        set = new List<BundleBuildInfo>();
                        assetToBundlesDict.Add(assetBuildInfo, set);
                    }

                    set.Add(bundleBuildInfo);
                }
            }

            return assetToBundlesDict;
        }

        /// <summary>
        /// 获取 冗余资源->所属的资源包列表 的字典 与 资源包->包含的冗余资源列表 的字典
        /// </summary>
        private static void GetRedundancyDict(Dictionary<AssetBuildInfo, List<BundleBuildInfo>> assetToBundlesDict,
            out Dictionary<AssetBuildInfo, List<BundleBuildInfo>> redundancyBundleDict,
            out Dictionary<BundleBuildInfo, List<AssetBuildInfo>> redundancyAssetDict)
        {
            //冗余资源->所属的资源包列表
            redundancyBundleDict = new Dictionary<AssetBuildInfo, List<BundleBuildInfo>>();

            //资源包->包含的冗余资源列表
            redundancyAssetDict = new Dictionary<BundleBuildInfo, List<AssetBuildInfo>>();

            foreach (KeyValuePair<AssetBuildInfo, List<BundleBuildInfo>> pair in assetToBundlesDict)
            {
                if (pair.Value.Count >= 2)
                {
                    //此资源被2个及以上数量的资源包所包含，是冗余资源
                    redundancyBundleDict.Add(pair.Key, pair.Value);

                    foreach (BundleBuildInfo bundleBuildInfo in pair.Value)
                    {
                        //放入此资源包的冗余资源列表中
                        if (!redundancyAssetDict.TryGetValue(bundleBuildInfo, out List<AssetBuildInfo> list))
                        {
                            list = new List<AssetBuildInfo>();
                            redundancyAssetDict.Add(bundleBuildInfo, list);
                        }

                        list.Add(pair.Key);

                        //从原本所在的资源包里删掉
                        bundleBuildInfo.Assets.Remove(pair.Key);
                    }


                }
            }
        }

        /// <summary>
        /// 生成冗余资源包
        /// </summary>
        private static List<BundleBuildInfo> GenRedundancyBundles(
            Dictionary<AssetBuildInfo, List<BundleBuildInfo>> redundancyBundleDict,
            Dictionary<BundleBuildInfo, List<AssetBuildInfo>> redundancyAssetDict)
        {
            //记录已递归遍历过的冗余资源
            HashSet<AssetBuildInfo> lookedAssetSet = new HashSet<AssetBuildInfo>();

            //记录已递归遍历过的资源包
            HashSet<BundleBuildInfo> lookedBundleSet = new HashSet<BundleBuildInfo>();

            //冗余资源包列表
            List<BundleBuildInfo> redundancyBundles = new List<BundleBuildInfo>();


            //递归遍历所有冗余资源
            foreach (KeyValuePair<AssetBuildInfo, List<BundleBuildInfo>> pair in redundancyBundleDict)
            {
                List<AssetBuildInfo> redundancyBundleAssets = new List<AssetBuildInfo>();

                //从作为递归起点的冗余资源开始遍历，找到所有能追踪到的冗余资源
                RecursiveRedundancyAsset(pair.Key,
                    lookedAssetSet, lookedBundleSet,
                    redundancyAssetDict, redundancyBundleDict,
                    redundancyBundleAssets);

                if (redundancyBundleAssets.Count == 0)
                {
                    //数量为0表示此冗余资源已经在之前被处理过了
                    continue;
                }

                //递归遍历结束 此时已经找到了所有从起点的冗余资源出发能追踪到的冗余资源了
                //将这些冗余资源放进同一个冗余资源包中
                string group = pair.Value[0].Group;
                BundleBuildInfo redundancyBundle =
                    new BundleBuildInfo(null, $"redundancy_{redundancyBundles.Count}.bundle", group, false);
                redundancyBundle.Assets = redundancyBundleAssets;

                redundancyBundles.Add(redundancyBundle);

            }

            return redundancyBundles;
        }

        private static void RecursiveRedundancyAsset(AssetBuildInfo assetBuildInfo,
            HashSet<AssetBuildInfo> lookedAssetSet,
            HashSet<BundleBuildInfo> lookedBundleSet,
            Dictionary<BundleBuildInfo, List<AssetBuildInfo>> redundancyAssetDict,
            Dictionary<AssetBuildInfo, List<BundleBuildInfo>> redundancyBundleDict,
            List<AssetBuildInfo> redundancyBundleAssets)
        {
            if (lookedAssetSet.Contains(assetBuildInfo))
            {
                //已递归遍历过 直接返回
                return;
            }

            lookedAssetSet.Add(assetBuildInfo);

            //放入待构建冗余资源包的资源列表中
            redundancyBundleAssets.Add(assetBuildInfo);

            //递归遍历包含此冗余资源的所有资源包
            List<BundleBuildInfo> bundles = redundancyBundleDict[assetBuildInfo];
            foreach (BundleBuildInfo bundleBuildInfo in bundles)
            {
                RecursiveRedundancyBundle(bundleBuildInfo,
                    lookedAssetSet, lookedBundleSet,
                    redundancyAssetDict, redundancyBundleDict,
                    redundancyBundleAssets);
            }
        }


        private static void RecursiveRedundancyBundle(BundleBuildInfo bundleBuildInfo,
            HashSet<AssetBuildInfo> lookedAssetSet,
            HashSet<BundleBuildInfo> lookedBundleSet,
            Dictionary<BundleBuildInfo, List<AssetBuildInfo>> redundancyAssetDict,
            Dictionary<AssetBuildInfo, List<BundleBuildInfo>> redundancyBundleDict,
            List<AssetBuildInfo> redundancyBundleAssets)
        {
            if (lookedBundleSet.Contains(bundleBuildInfo))
            {
                //已递归遍历过 直接返回
                return;
            }

            lookedBundleSet.Add(bundleBuildInfo);

            //递归遍历此资源包中的所有冗余资源
            List<AssetBuildInfo> assets = redundancyAssetDict[bundleBuildInfo];
            foreach (AssetBuildInfo assetBuildInfo in assets)
            {
                RecursiveRedundancyAsset(assetBuildInfo,
                    lookedAssetSet, lookedBundleSet,
                    redundancyAssetDict, redundancyBundleDict,
                    redundancyBundleAssets);
            }

        }
    }
}