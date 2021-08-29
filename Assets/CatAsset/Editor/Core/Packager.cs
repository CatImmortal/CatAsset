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
        public static void ExecutePackagePipeline(string outputPath, BuildAssetBundleOptions options, BuildTarget targetPlatform,int manifestVersion,bool isCopyToStreamingAssets,bool isAnalyzeRedundancy)
        {
            //获取最终打包输出目录
            string finalOutputPath = GetFinalOutputPath(outputPath, targetPlatform, manifestVersion);

            //创建打包输出目录
            CreateOutputPath(finalOutputPath);


            //获取AssetBundleBuildList，然后打包AssetBundle
            List<AssetBundleBuild> abBuildList = Util.PkgRuleCfg.GetAssetBundleBuildList();
            AssetBundleManifest unityManifest = PackageAssetBundles(finalOutputPath, abBuildList,options,targetPlatform);

            //生成资源清单文件
            GenerateAssetManifestFile(finalOutputPath, abBuildList,unityManifest,manifestVersion);

            //资源清单版本号自增
            ChangeManifestVersion();

            //将资源复制到StreamingAssets下
            if (isCopyToStreamingAssets)
            {
                CopyToStreamingAssets(finalOutputPath);
            }
        }

        /// <summary>
        /// 获取最终打包输出目录
        /// </summary>
        private static string GetFinalOutputPath(string outputPath, BuildTarget targetPlatform, int manifestVersion)
        {
            string result = outputPath += "\\" + Application.version + "\\" + manifestVersion + "\\" + targetPlatform; ;
            return result;
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
        private static AssetBundleManifest PackageAssetBundles(string outputPath,List<AssetBundleBuild> abBuildList, BuildAssetBundleOptions options, BuildTarget targetPlatform)
        {
            AssetBundleManifest unityManifest =  BuildPipeline.BuildAssetBundles(outputPath, abBuildList.ToArray(), options, targetPlatform);

            DirectoryInfo dirInfo = new DirectoryInfo(outputPath);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                if (file.Name == targetPlatform.ToString() || file.Extension == ".manifest")
                {
                    //删除manifest文件
                    file.Delete();
                }
            }

            return unityManifest;
        }
  
        /// <summary>
        /// 生成资源清单文件
        /// </summary>
        private static void GenerateAssetManifestFile(string outputPath, List<AssetBundleBuild> abBuildList, AssetBundleManifest unityManifest,int manifestVersion)
        {

            CatAssetManifest manifest = new CatAssetManifest();
            manifest.GameVersion = Application.version;
            manifest.ManifestVersion = manifestVersion;

            manifest.AssetBundles = new AssetBundleManifestInfo[abBuildList.Count];
            for (int i = 0; i < abBuildList.Count; i++)
            {
                AssetBundleBuild abBulid = abBuildList[i];

                AssetBundleManifestInfo abInfo = new AssetBundleManifestInfo();
                manifest.AssetBundles[i] = abInfo;

                abInfo.AssetBundleName = abBulid.assetBundleName;

                string fullPath = outputPath + "\\" + abInfo.AssetBundleName;
                FileInfo fileInfo = new FileInfo(fullPath);
                abInfo.Length = fileInfo.Length;

                abInfo.Hash = unityManifest.GetAssetBundleHash(abInfo.AssetBundleName);

                abInfo.IsScene = abBulid.assetNames[0].EndsWith(".unity");  //判断是否为场景ab

                abInfo.Assets = new AssetManifestInfo[abBulid.assetNames.Length];
                for (int j = 0; j < abBulid.assetNames.Length; j++)
                {
                    AssetManifestInfo assetInfo = new AssetManifestInfo();
                    abInfo.Assets[j] = assetInfo;

                    assetInfo.AssetName = abBulid.assetNames[j];
                    assetInfo.Dependencies = Util.GetDependencies(assetInfo.AssetName,false);  //依赖列表不进行递归记录 因为加载的时候会对依赖进行递归加载
                }

               
            }

            //写入清单文件json
            string json = CatJson.JsonParser.ToJson(manifest);
            using (StreamWriter sw = new StreamWriter(outputPath + "\\CatAssetManifest.json"))
            {
                sw.Write(json);
            }
            
        }
    
        /// <summary>
        /// 修改资源清单版本号
        /// </summary>
        private static void ChangeManifestVersion()
        {
            //自增
            Util.PkgCfg.ManifestVersion++;
        }
    
        /// <summary>
        /// 将资源复制到StreamingAssets下
        /// </summary>
        private static void CopyToStreamingAssets(string outputPath)
        {
            //StreamingAssets目录已存在就清空
            if (Directory.Exists(Application.streamingAssetsPath))
            {

                DirectoryInfo dirInfo = new DirectoryInfo(Application.streamingAssetsPath);
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }
            }
            else
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            DirectoryInfo outputDirInfo = new DirectoryInfo(outputPath);


            foreach (FileInfo item in outputDirInfo.GetFiles())
            {
                item.CopyTo(Application.streamingAssetsPath + "/" + item.Name);
            }

            AssetDatabase.Refresh();
            Debug.Log("已将资源复制到StreamingAssets目录下");
        }
    }

}
