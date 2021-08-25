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

        /// <summary>
        /// 执行打包管线
        /// </summary>
        public static void ExecutePackagePipeline(string outputPath, BuildAssetBundleOptions options, BuildTarget targetPlatform)
        {
            //创建打包输出目录
            outputPath += "/" + targetPlatform;
            CreateOutputPath(outputPath);

            //获取AssetBundleBuildList，然后打包AssetBundle
            List<AssetBundleBuild> abBuildList = Util.PkgRuleCfg.GetAssetBundleBuildList();
            PackageAssetBundles(outputPath,abBuildList,options,targetPlatform);

            //生成AssetManifest文件
            GenerateAssetManifestFile(outputPath, abBuildList);
        }

        /// <summary>
        /// 创建打包输出目录
        /// </summary>
        private static void CreateOutputPath(string outputPath)
        {
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
            }
        }
    
        /// <summary>
        /// 打包AssetBundle
        /// </summary>
        private static void PackageAssetBundles(string outputPath,List<AssetBundleBuild> abBuildList, BuildAssetBundleOptions options, BuildTarget targetPlatform)
        {
            BuildPipeline.BuildAssetBundles(outputPath, abBuildList.ToArray(), options, targetPlatform);

            DirectoryInfo dirInfo = new DirectoryInfo(outputPath);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                if (file.Name == targetPlatform.ToString() || file.Extension == ".manifest")
                {
                    //删除manifest文件
                    file.Delete();
                }
            }
        }
  
        /// <summary>
        /// 生成AssetManifest文件
        /// </summary>
        private static void GenerateAssetManifestFile(string outputPath, List<AssetBundleBuild> abBuildList)
        {

        }
    }

}
