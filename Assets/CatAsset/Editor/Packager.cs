using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 打包器
    /// </summary>
    public static class Packager
    {
        [MenuItem("CatAsset/测试打包AssetBundle")]
        private static void TestBuildAB()
        {
            BuildAssetBundle(null, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
        }

        /// <summary>
        /// 打包AssetBundle
        /// </summary>
        public static void BuildAssetBundle(string outputPath, BuildAssetBundleOptions options, BuildTarget targetPlatform)
        {
            PackageRuleConfig config = Util.GetPackageRuleConfig();

            List<AssetBundleBuild> abBuildList = new List<AssetBundleBuild>();

            foreach (PackageRule rule in config.Rules)
            {
                Func<string, AssetBundleBuild[]> func = AssetCollectFuncs.FuncDict[rule.Mode];
                AssetBundleBuild[] abBuilds = func(rule.Directory);
                abBuildList.AddRange(abBuilds);

            }

            //foreach (var item in abBuildList)
            //{
            //    Debug.Log(item.assetBundleName);
            //    foreach (var item2 in item.assetNames)
            //    {
            //        Debug.Log(item2);
            //    }

            //    Debug.Log("-------------");
            //}

            outputPath = Directory.GetCurrentDirectory() + "/AssetBundleOutput";
            targetPlatform = BuildTarget.StandaloneWindows64;

            outputPath += "/" + targetPlatform;

            //打包目录已存在就清空 然后重新创建
            DirectoryInfo dirInfo;

            if (Directory.Exists(outputPath))
            {
                
                dirInfo = new DirectoryInfo(outputPath);
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }
            }
            Directory.CreateDirectory(outputPath);
            dirInfo = new DirectoryInfo(outputPath);


            BuildPipeline.BuildAssetBundles(outputPath, abBuildList.ToArray(),options,targetPlatform);

            Debug.Log("打包ab完毕");

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                if (file.Name == targetPlatform.ToString() || file.Extension == ".manifest")
                {
                    //删除manifest文件
                    file.Delete();
                }
            }

        }

      
    }

}
