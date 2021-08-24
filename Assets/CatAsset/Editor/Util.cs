using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CatAsset.Editor
{
    public static class Util
    {
        public static PackageConfig PkgCfg
        {
            get;
            private set;
        }

        public static PackageRuleConfig PkgRuleCfg
        {
            get;
            private set;
        }

        static Util()
        {
            PkgCfg = GetConfigAsset<PackageConfig>();
            PkgRuleCfg = GetConfigAsset<PackageRuleConfig>();
        }

        /// <summary>
        /// 创建配置
        /// </summary>
        public static void CreateConfigAsset<T>() where T : ScriptableObject
        {
            string typeName = typeof(T).Name;
            string[] paths = AssetDatabase.FindAssets("t:" + typeName);
            if (paths.Length >= 1)
            {
                string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                EditorUtility.DisplayDialog("警告", $"已存在{typeName}，路径:{path}", "确认");
                return;
            }

            T config = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(config, $"Assets/{typeName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("提示", $"{typeName}创建完毕，路径:Assets/{typeName}.asset", "确认");
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        private static T GetConfigAsset<T>() where T : ScriptableObject
        {
            string typeName = typeof(T).Name;
            string[] paths = AssetDatabase.FindAssets("t:" + typeName);
            if (paths.Length == 0)
            {
                throw new Exception("不存在" + typeName);
            }
            if (paths.Length > 1)
            {
                throw new Exception(typeName + "数量大于1");

            }
            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            T config = AssetDatabase.LoadAssetAtPath<T>(path);

            return config;
        }

        

        /// <summary>
        /// 获取需要打包的AssetBundleBuild列表
        /// </summary>
        public static List<AssetBundleBuild> GetAssetBundleBuildList(bool isAnalyzeRedundancyAssets = true)
        {
            List<AssetBundleBuild> abBuildList = new List<AssetBundleBuild>();

            //获取被显式打包的Asset和AssetBundle
            foreach (PackageRule rule in PkgRuleCfg.Rules)
            {
                Func<string, AssetBundleBuild[]> func = AssetCollectFuncs.FuncDict[rule.Mode];
                AssetBundleBuild[] abBuilds = func(rule.Directory);
                if (abBuilds != null)
                {
                    abBuildList.AddRange(abBuilds);
                }
               
            }

            if (abBuildList.Count > 0 && isAnalyzeRedundancyAssets)
            {
                AnalyzeRedundancyAssets(abBuildList);
            }

            return abBuildList;
        }

        /// <summary>
        /// 冗余资源分析并消除所有隐式依赖
        /// </summary>
        private static void AnalyzeRedundancyAssets(List<AssetBundleBuild> abBuildList)
        {
            //所有被显式打包的Asset名和它的AssetBundle名
            Dictionary<string, string> pakageAssetInfoDict = new Dictionary<string, string>();

            //AssetBundle名与AssetBundleBuild
            Dictionary<string, AssetBundleBuild> bundleInfoDict = new Dictionary<string, AssetBundleBuild>();

            //AssetBundle名与其Asset列表
            Dictionary<string, List<string>> bundleAssetNameListDict = new Dictionary<string, List<string>>();

            //先把显式打包的Asset都放入Set中
            foreach (AssetBundleBuild abBuild in abBuildList)
            {
                bundleInfoDict.Add(abBuild.assetBundleName, abBuild);
                bundleAssetNameListDict.Add(abBuild.assetBundleName, new List<string>(abBuild.assetNames));

                foreach (string assetName in abBuild.assetNames)
                {
                    pakageAssetInfoDict.Add(assetName,abBuild.assetBundleName);
                }
            }

            //被依赖且未显式打包的Asset和依赖它的AssetBundle的字典
            Dictionary<string, HashSet<string>> dependencyInfoDict = new Dictionary<string, HashSet<string>>();

            //遍历显式打包的Asset的依赖
            foreach (KeyValuePair<string, string> item in pakageAssetInfoDict)
            {
                string assetName = item.Key;
                string assetBundleName = item.Value;

                string[] dependencies = AssetDatabase.GetDependencies(assetName);

                foreach (string dependencyName in dependencies)
                {
                    if (dependencyName.EndsWith(".cs"))
                    {
                        //跳过c#代码
                        continue;
                    }

                    if (pakageAssetInfoDict.ContainsKey(dependencyName))
                    {
                        //跳过已显式打包的Asset
                        continue;
                    }

                    //记录隐式依赖信息
                    HashSet<string> bundleNameSet;
                    if (!dependencyInfoDict.TryGetValue(dependencyName,out bundleNameSet))
                    {
                        bundleNameSet = new HashSet<string>();
                        dependencyInfoDict.Add(dependencyName, bundleNameSet);
                    }
                    bundleNameSet.Add(assetBundleName);

                }
            }

            //冗余资源名列表
            List<string> redundancyAssetList = new List<string>();

            //处理冗余资源
            foreach (KeyValuePair<string, HashSet<string>> item in dependencyInfoDict)
            {
                if (item.Value.Count > 1)
                {
                    //Debug.Log($"{item.Key}冗余了");
                    redundancyAssetList.Add(item.Key);
                }
                else
                {
                    //非冗余的隐式依赖
                    foreach (string abName in item.Value)
                    {
                        bundleAssetNameListDict[abName].Add(item.Key);
                    }
                }
            }

            //重建abBuildList信息 将非冗余的隐式依赖 放入abBuild.AssetNames中成为显式的
            for (int i = 0; i < abBuildList.Count; i++)
            {
                AssetBundleBuild abBuild = abBuildList[i];
                abBuild.assetNames = bundleAssetNameListDict[abBuild.assetBundleName].ToArray();
                abBuildList[i] = abBuild;
            }

            if (redundancyAssetList.Count > 0)
            {
                //冗余资源独立打包
                //todo:考虑过大拆分
                AssetBundleBuild redundancyABBuild = default;
                redundancyABBuild.assetBundleName = "common.bundle";
                redundancyABBuild.assetNames = redundancyAssetList.ToArray();
                abBuildList.Add(redundancyABBuild);
            }



        }
    }

}
