using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
namespace CatAsset.Editor
{
    /// <summary>
    /// Asset收集方法集，定义了不同打包模式下对Asset的收集方法
    /// </summary>
    public static class AssetCollectFuncs
    {
        public static Dictionary<PackageMode, Func<string, AssetBundleBuild[]>> FuncDict = new Dictionary<PackageMode, Func<string, AssetBundleBuild[]>>();

        static AssetCollectFuncs()
        {
            //打包模式1 将指定文件夹下所有Asset打包为一个Bundle
            FuncDict.Add(PackageMode.Model_1, (directory) =>
            {
                List<string> assetPaths = new List<string>();

                if (Directory.Exists(directory))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(directory);
                    FileInfo[] files = dirInfo.GetFiles("*",SearchOption.AllDirectories);  //递归获取所有文件
                    foreach (FileInfo file in files)
                    {
                        if (file.Extension == ".meta")
                        {
                            //跳过meta文件
                            continue;
                        }
                        int index = file.FullName.IndexOf("Assets\\");
                        string path = file.FullName.Substring(index);
                        assetPaths.Add(path);
                    }
                }

                AssetBundleBuild abBuild = default;
                abBuild.assetNames = assetPaths.ToArray();
                abBuild.assetBundleName = directory.Replace("/", "_") + ".bundle";

                return new AssetBundleBuild[] { abBuild };
            });


            FuncDict.Add(PackageMode.Model_2, (directory) =>
            {

                return null;
            });
        }
    }

}
