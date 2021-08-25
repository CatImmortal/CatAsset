using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CatAsset.Editor
{
    /// <summary>
    /// 冗余资源分析器
    /// </summary>
    public static class RedundancyAnalyzer
    {
        /// <summary>
        /// 执行冗余资源分析管线
        /// </summary>
        public static void ExecuteRedundancyAnalyzePipeline(List<AssetBundleBuild> abBuildList)
        {
            //显式打包的Asset和所属的AssetBundle
            Dictionary<string, string> pakageAssetInfoDict = new Dictionary<string, string>();

            //AssetBundle与所属的AssetBundleBuild
            Dictionary<string, AssetBundleBuild> bundleInfoDict = new Dictionary<string, AssetBundleBuild>();

            //AssetBundle与其中的Asset列表
            Dictionary<string, List<string>> bundleAssetsDict = new Dictionary<string, List<string>>();

            //初始化冗余分析需要用到的数据
            InitAnalyzetData(abBuildList, pakageAssetInfoDict, bundleInfoDict, bundleAssetsDict);

            //被隐式依赖的Asset和依赖它的AssetBundle
            Dictionary<string, HashSet<string>> dependencyInfoDict = new Dictionary<string, HashSet<string>>();

            //获取隐式依赖数据
            GetDependencyData(pakageAssetInfoDict, dependencyInfoDict);

            //冗余资源列表
            List<string> redundancyAssetList = new List<string>();

            //检测冗余资源
            CheckRedundancyAssets(bundleAssetsDict, dependencyInfoDict, redundancyAssetList);

            //重建abBuildList信息
            RebuildAssetBundleBuildList(abBuildList, bundleAssetsDict);

            //追加冗余资源的AssetBundleBuild
            AppendRedundancyBundle(abBuildList, redundancyAssetList);
        }

        /// <summary>
        /// 初始化分析冗余需要用到的数据
        /// </summary>
        private static void InitAnalyzetData(List<AssetBundleBuild> abBuildList, Dictionary<string, string> pakageAssetInfoDict, Dictionary<string, AssetBundleBuild> bundleInfoDict, Dictionary<string, List<string>> bundleAssetsDict)
        {

            foreach (AssetBundleBuild abBuild in abBuildList)
            {
                bundleInfoDict.Add(abBuild.assetBundleName, abBuild);
                bundleAssetsDict.Add(abBuild.assetBundleName, new List<string>(abBuild.assetNames));

                foreach (string assetName in abBuild.assetNames)
                {
                    pakageAssetInfoDict.Add(assetName, abBuild.assetBundleName);
                }
            }
        }

        /// <summary>
        /// 获取隐式依赖数据
        /// </summary>
        private static void GetDependencyData(Dictionary<string, string> pakageAssetInfoDict, Dictionary<string, HashSet<string>> dependencyInfoDict)
        {
            //遍历显式打包的Asset的依赖
            foreach (KeyValuePair<string, string> item in pakageAssetInfoDict)
            {
                string assetName = item.Key;
                string assetBundleName = item.Value;

                string[] dependencies = Util.GetDependencies(assetName);

                foreach (string dependencyName in dependencies)
                {
                    if (pakageAssetInfoDict.ContainsKey(dependencyName))
                    {
                        //跳过已显式打包的Asset
                        continue;
                    }

                    //记录隐式依赖信息
                    if (!dependencyInfoDict.TryGetValue(dependencyName, out HashSet<string> bundleNameSet))
                    {
                        bundleNameSet = new HashSet<string>();
                        dependencyInfoDict.Add(dependencyName, bundleNameSet);
                    }
                    bundleNameSet.Add(assetBundleName);

                }
            }
        }

        /// <summary>
        /// 检测冗余资源
        /// </summary>
        private static void CheckRedundancyAssets(Dictionary<string, List<string>> bundleAssetsDict, Dictionary<string, HashSet<string>> dependencyInfoDict, List<string> redundancyAssetList)
        {
            foreach (KeyValuePair<string, HashSet<string>> item in dependencyInfoDict)
            {
                if (item.Value.Count >= 2)
                {
                    //被2个以上AssetBundle隐式依赖即是冗余资源
                    redundancyAssetList.Add(item.Key);
                }
                else
                {
                    //非冗余的隐式依赖 将其附加到依赖它的AssetBundle中成为显式打包的资源
                    foreach (string abName in item.Value)
                    {
                        bundleAssetsDict[abName].Add(item.Key);
                    }
                }
            }
        }

        /// <summary>
        /// 重建abBuildList信息 将非冗余的隐式依赖 变成显式打包的
        /// </summary>
        private static void RebuildAssetBundleBuildList(List<AssetBundleBuild> abBuildList, Dictionary<string, List<string>> bundleAssetsDict)
        {
            for (int i = 0; i < abBuildList.Count; i++)
            {
                AssetBundleBuild abBuild = abBuildList[i];
                abBuild.assetNames = bundleAssetsDict[abBuild.assetBundleName].ToArray();
                abBuildList[i] = abBuild;
            }
        }

        /// <summary>
        /// 追加冗余资源的AssetBundleBuild
        /// </summary>
        private static void AppendRedundancyBundle(List<AssetBundleBuild> abBuildList, List<string> redundancyAssetList)
        {
            if (redundancyAssetList.Count > 0)
            {
                //todo:考虑过大拆分
                AssetBundleBuild redundancyABBuild = default;
                redundancyABBuild.assetBundleName = "common.bundle";
                redundancyABBuild.assetNames = redundancyAssetList.ToArray();
                abBuildList.Add(redundancyABBuild);
            }
        }
    }
}

