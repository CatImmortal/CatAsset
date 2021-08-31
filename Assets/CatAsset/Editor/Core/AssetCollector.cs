using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
namespace CatAsset.Editor
{

    /// <summary>
    /// Asset收集器
    /// 定义了不同打包模式下对Asset的收集方法
    /// </summary>
    public static class AssetCollector
    {
        /// <summary>
        /// 打包模式与Asset收集方法
        /// </summary>
        private static Dictionary<PackageMode, Func<PackageRule, AssetBundleBuild[]>> collectorFuncDict = new Dictionary<PackageMode, Func<PackageRule, AssetBundleBuild[]>>();

        /// <summary>
        /// 要排除的文件后缀名集合
        /// </summary>
        private static HashSet<string> excludeExtension = new HashSet<string>();

        /// <summary>
        /// AssetBundle与其资源组
        /// </summary>
        private static Dictionary<string, string> AssetBundleGroupDict = new Dictionary<string, string>();

        static AssetCollector()
        {
            excludeExtension.Add(".meta");
            excludeExtension.Add(".cs");
            excludeExtension.Add(".asmdef");
            excludeExtension.Add(".giparams");

            collectorFuncDict.Add(PackageMode.Mode_1, Model_1_CollecteFunc);
        }

        /// <summary>
        /// 根据打包模式获取AssetBundleBuild列表
        /// </summary>
        public static AssetBundleBuild[] GetAssetBundleBuilds(PackageRule rule)
        {
            if (collectorFuncDict.TryGetValue(rule.Mode, out Func<PackageRule, AssetBundleBuild[]> func))
            {
                return func(rule);
            }

            throw new Exception($"未定义打包模式{rule.Mode}下的Asset收集方法");
        }

        /// <summary>
        /// 获取AssetBundle的资源组
        /// </summary>
        public static string GetAssetBundleGroup(string assetBundleName)
        {
            if (AssetBundleGroupDict.ContainsKey(assetBundleName))
            {
                return AssetBundleGroupDict[assetBundleName];
            }

            return null;
        }

        /// <summary>
        /// 打包模式1 将指定文件夹下所有Asset打包为一个Bundle
        /// </summary>
        private static AssetBundleBuild[] Model_1_CollecteFunc(PackageRule rule)
        {
            List<string> assetPaths = new List<string>();

            if (Directory.Exists(rule.Directory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(rule.Directory);
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);  //递归获取所有文件
                foreach (FileInfo file in files)
                {
                    if (excludeExtension.Contains(file.Extension))
                    {
                        //跳过不该打包的文件
                        continue;
                    }
                    int index = file.FullName.IndexOf("Assets\\");
                    string path = file.FullName.Substring(index);
                    assetPaths.Add(path.Replace('\\', '/'));
                }

                if (assetPaths.Count == 0)
                {
                    //空包不打
                    return null;
                }

                AssetBundleBuild abBuild = default;
                abBuild.assetNames = assetPaths.ToArray();
                abBuild.assetBundleName = rule.Directory.Replace("Assets/Res/", "") + ".bundle";
                abBuild.assetBundleName = abBuild.assetBundleName.ToLower();
                AssetBundleGroupDict[abBuild.assetBundleName] = rule.Group;

                return new AssetBundleBuild[] { abBuild };
            }

            return null;
        }
   

    }

}
