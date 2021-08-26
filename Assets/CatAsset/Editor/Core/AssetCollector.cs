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
        private static Dictionary<PackageMode, Func<string, AssetBundleBuild[]>> collectorFuncDict = new Dictionary<PackageMode, Func<string, AssetBundleBuild[]>>();

        /// <summary>
        /// 要排除的文件后缀名
        /// </summary>
        private static HashSet<string> excludeExtension = new HashSet<string>();

        static AssetCollector()
        {
            excludeExtension.Add(".meta");
            excludeExtension.Add(".cs");


            collectorFuncDict.Add(PackageMode.Model_1, Model_1_CollecteFunc);
        }

        /// <summary>
        /// 根据打包模式获取AssetBundleBuild列表
        /// </summary>
        public static AssetBundleBuild[] GetAssetBundleBuilds(PackageMode model,string directory)
        {
            if (collectorFuncDict.TryGetValue(model, out Func<string, AssetBundleBuild[]> func))
            {
                return func(directory);
            }

            throw new Exception($"未定义打包模式{model}下的Asset收集方法");
        }

        /// <summary>
        /// 打包模式1 将指定文件夹下所有Asset打包为一个Bundle
        /// </summary>
        private static AssetBundleBuild[] Model_1_CollecteFunc(string directory)
        {
            List<string> assetPaths = new List<string>();

            if (Directory.Exists(directory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);  //递归获取所有文件
                foreach (FileInfo file in files)
                {
                    if (excludeExtension.Contains(file.Extension))
                    {
                        //跳过meta和cshape代码文件
                        continue;
                    }
                    int index = file.FullName.IndexOf("Assets\\");
                    string path = file.FullName.Substring(index);
                    assetPaths.Add(path.Replace('\\', '/'));
                }

                AssetBundleBuild abBuild = default;
                abBuild.assetNames = assetPaths.ToArray();
                abBuild.assetBundleName = directory.Replace("Assets/Res/", "") + ".bundle";
                abBuild.assetBundleName = abBuild.assetBundleName.ToLower();
                return new AssetBundleBuild[] { abBuild };
            }

            return null;
        }
   

    }

}
