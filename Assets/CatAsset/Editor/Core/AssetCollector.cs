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

            collectorFuncDict.Add(PackageMode.NAssetToOneBundle, NAssetToOneBundle);
            collectorFuncDict.Add(PackageMode.TopDirectoryNAssetToOneBundle, TopDirectoryNAssetToOneBundle);
            collectorFuncDict.Add(PackageMode.NAssetToNBundle, NAssetToNBundle);
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

            Debug.LogError($"未定义打包模式{rule.Mode}下的Asset收集方法");
            return null;
        }

        /// <summary>
        /// 获取AssetBundle所在的资源组
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
        /// 获取AsestBundle名
        /// </summary>
        private static string GetAssetBundleName(string name)
        {
            string assetBundleName = name.Replace("Assets/","").Replace('/','_') + ".bundle";
            assetBundleName = assetBundleName.ToLower();
            return assetBundleName;
        }

        /// <summary>
        /// 将指定文件夹下所有Asset打包为一个Bundle
        /// </summary>
        private static AssetBundleBuild[] NAssetToOneBundle(PackageRule rule)
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
                    string path = file.FullName.Substring(index); //获取Asset开头的资源全路径
                    assetPaths.Add(path.Replace('\\', '/'));
                }

                if (assetPaths.Count == 0)
                {
                    //空包不打
                    return null;
                }

                AssetBundleBuild abBuild = default;
                abBuild.assetNames = assetPaths.ToArray();
                abBuild.assetBundleName = GetAssetBundleName(rule.Directory);
                AssetBundleGroupDict[abBuild.assetBundleName] = rule.Group;

                return new AssetBundleBuild[] { abBuild };
            }

            return null;
        }

        /// <summary>
        /// 对指定文件夹下所有一级子目录各自使用NAssetToOneBundle打包为一个bundle
        /// </summary>
        private static AssetBundleBuild[] TopDirectoryNAssetToOneBundle(PackageRule rule)
        {
            List<string> assetPaths = new List<string>();
            List<AssetBundleBuild> abBulidList = new List<AssetBundleBuild>();

            if (Directory.Exists(rule.Directory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(rule.Directory);

                //获取所有一级目录
                DirectoryInfo[] topDirectorys = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

                foreach (DirectoryInfo topDirInfo in topDirectorys)
                {
                    //递归获取所有文件
                    FileInfo[] files = topDirInfo.GetFiles("*", SearchOption.AllDirectories);  
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
                        continue;
                    }

                    AssetBundleBuild abBuild = default;
                    abBuild.assetNames = assetPaths.ToArray();
                    assetPaths.Clear();
                    abBuild.assetBundleName = GetAssetBundleName(rule.Directory + "/" + topDirInfo.Name);
                    AssetBundleGroupDict[abBuild.assetBundleName] = rule.Group;

                    abBulidList.Add(abBuild);
                }

                if (abBulidList.Count > 0)
                {
                    return abBulidList.ToArray();
                }

                return null;
            }

            return null;
        }

        /// <summary>
        /// 对指定文件夹下所有asset各自打包为一个bundle
        /// </summary>
        private static AssetBundleBuild[] NAssetToNBundle(PackageRule rule)
        {

            List<AssetBundleBuild> abBulidList = new List<AssetBundleBuild>();

            if (Directory.Exists(rule.Directory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(rule.Directory);

                //递归获取所有文件
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    if (excludeExtension.Contains(file.Extension))
                    {
                        //跳过不该打包的文件
                        continue;
                    }
                 

                    int assetsIndex = file.FullName.IndexOf("Assets\\");
                    string assetName = file.FullName.Substring(assetsIndex).Replace('\\', '/');

                    int suffixIndex = assetName.LastIndexOf('.');
                    string abName = GetAssetBundleName(assetName.Remove(suffixIndex));  //ab名 是不带后缀的asset名

                    AssetBundleBuild abBuild = default;
                    abBuild.assetNames = new string[] { assetName };
                    abBuild.assetBundleName = abName;
                    AssetBundleGroupDict[abBuild.assetBundleName] = rule.Group;
                    abBulidList.Add(abBuild);
                }

                if (abBulidList.Count > 0)
                {
                    return abBulidList.ToArray();
                }

                return null;
            }

            return null;
        }
    }

}
