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
        [MenuItem("CatAsset/测试冗余分析")]
        private static void TestBuildAB()
        {
            Util.GetAssetBundleBuildList();
        }

        /// <summary>
        /// 打包AssetBundle
        /// </summary>
        public static void PackageAssetBundle(string outputPath, BuildAssetBundleOptions options, BuildTarget targetPlatform)
        {
           
            outputPath += "/" + targetPlatform;

            //打包目录已存在就清空
            DirectoryInfo dirInfo;
            if (Directory.Exists(outputPath))
            {
                
                dirInfo = new DirectoryInfo(outputPath);
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }
            }
            else
            {
                Directory.CreateDirectory(outputPath);
                dirInfo = new DirectoryInfo(outputPath);
            }


            List<AssetBundleBuild> abBuildList = Util.GetAssetBundleBuildList();
            BuildPipeline.BuildAssetBundles(outputPath, abBuildList.ToArray(),options,targetPlatform);

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
