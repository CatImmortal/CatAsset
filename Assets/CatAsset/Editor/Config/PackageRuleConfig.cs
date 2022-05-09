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
            PkgUtil.CreateConfigAsset<PackageRuleConfig>();
        }

        /// <summary>
        /// 获取最终实际进行打包的AssetBundleBuild列表
        /// </summary>
        public  List<AssetBundleBuild> GetAssetBundleBuildList(bool isAnalyzeRedundancy = true)
        {
            List<AssetBundleBuild> abBuildList = new List<AssetBundleBuild>();

            Rules.Sort();

            //获取被显式打包的Asset和AssetBundle
            foreach (PackageRule rule in Rules)
            {
                AssetBundleBuild[] abBuilds = AssetCollector.GetAssetBundleBuilds(rule);
                if (abBuilds != null)
                {
                    abBuildList.AddRange(abBuilds);
                }

            }

            if (abBuildList.Count > 0 && isAnalyzeRedundancy)
            {
                //进行冗余分析
                RedundancyAnalyzer.ExecuteRedundancyAnalyzePipeline(abBuildList);
            }
            
            //拆分场景和非场景资源混在一起的包
            List<AssetBundleBuild> reuslt = new List<AssetBundleBuild>();
            for (int i = 0; i < abBuildList.Count; i++)
            {
                AssetBundleBuild abBulid = abBuildList[i];
                if (SplitSceneBundleAsset(abBulid,out AssetBundleBuild sceneBundle,out AssetBundleBuild notSceneBundle))
                {
                    //将场景和非场景混在一起的包拆开打包
                    reuslt.Add(sceneBundle);
                    
                    reuslt.Add(notSceneBundle);
                    AssetCollector.AddAssetBundleGroup(notSceneBundle.assetBundleName);
                }
                else
                {
                    reuslt.Add(abBulid);
                }
            }
            
            //排序所有Asset
            for (int i = 0; i < reuslt.Count; i++)
            {
                AssetBundleBuild abBulid = reuslt[i];
                Array.Sort(abBulid.assetNames);
            }
            
            
            return reuslt;
        }
        
        
        /// <summary>
        /// 拆分场景包和其依赖的非场景资源
        /// </summary>
        private static bool SplitSceneBundleAsset(AssetBundleBuild abBuild,out AssetBundleBuild sceneBundle,out AssetBundleBuild notSceneBundle)
        {
            sceneBundle = default;
            notSceneBundle = default;
            
            List<string> sceneNames = new List<string>();
            List<string> assetNames = new List<string>();

            for (int i = 0; i < abBuild.assetNames.Length; i++)
            {
                string assetName = abBuild.assetNames[i];
                if (assetName.EndsWith(".unity"))
                {
                    sceneNames.Add(assetName);
                }
                else
                {
                    assetNames.Add(assetName);
                }
            }

            if (sceneNames.Count > 0 && assetNames.Count > 0)
            {
                sceneBundle.assetBundleName = abBuild.assetBundleName;
                sceneBundle.assetNames = sceneNames.ToArray();

                string[] splitNames = abBuild.assetBundleName.Split('.');

                notSceneBundle.assetBundleName = $"{splitNames[0]}_res.{splitNames[1]}";
                notSceneBundle.assetNames = assetNames.ToArray();
                
                return true;
            }

            return false;
        }
    }
}

