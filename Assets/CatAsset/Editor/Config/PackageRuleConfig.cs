using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace CatAsset.Editor
{
    /// <summary>
    /// 打包规则配置
    /// </summary>
    public class PackageRuleConfig : ScriptableObject
    {
        [SerializeField]
        public List<PackageRule> Rules;

        [MenuItem("CatAsset/创建打包规则配置文件")]
        private static void CreateConfig()
        {
            Util.CreateConfigAsset<PackageRuleConfig>();
        }

        /// <summary>
        /// 获取实际进行打包的AssetBundleBuild列表
        /// </summary>
        public  List<AssetBundleBuild> GetAssetBundleBuildList(bool isAnalyzeRedundancyAssets = true)
        {
            List<AssetBundleBuild> abBuildList = new List<AssetBundleBuild>();

            //获取被显式打包的Asset和AssetBundle
            foreach (PackageRule rule in Rules)
            {
                AssetBundleBuild[] abBuilds = AssetCollector.GetAssetBundleBuilds(rule.Mode, rule.Directory);
                if (abBuilds != null)
                {
                    abBuildList.AddRange(abBuilds);
                }

            }

            if (abBuildList.Count > 0 && isAnalyzeRedundancyAssets)
            {
                //进行冗余分析
                RedundancyAnalyzer.ExecuteRedundancyAnalyzePipeline(abBuildList);
            }

            return abBuildList;
        }
    }
}

