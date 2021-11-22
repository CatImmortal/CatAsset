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
        public static void ExecutePackagePipeline(string outputPath, BuildAssetBundleOptions options, BuildTarget targetPlatform,int manifestVersion, bool isAnalyzeRedundancy, bool isCopyToStreamingAssets,string copyGroup)
        {
            //获取最终打包输出目录
            string finalOutputPath = GetFinalOutputPath(outputPath, targetPlatform, manifestVersion);

            //创建打包输出目录
            CreateOutputPath(finalOutputPath);

            //获取AssetBundleBuildList，然后打包AssetBundle
            List<AssetBundleBuild> abBuildList = PkgUtil.PkgRuleCfg.GetAssetBundleBuildList(isAnalyzeRedundancy);
            AssetBundleManifest unityManifest = PackageAssetBundles(finalOutputPath, abBuildList,options,targetPlatform);

            //生成资源清单文件
            CatAssetManifest manifest = GenerateManifestFile(finalOutputPath, abBuildList,unityManifest,manifestVersion);

            //资源清单版本号自增
            ChangeManifestVersion();

            //将资源复制到StreamingAssets下
            if (isCopyToStreamingAssets)
            {
                CopyToStreamingAssets(finalOutputPath,copyGroup,manifest);
            }

            
        }

        /// <summary>
        /// 获取最终打包输出目录
        /// </summary>
        private static string GetFinalOutputPath(string outputPath, BuildTarget targetPlatform, int manifestVersion)
        {
            string dir = Application.version + "_" + manifestVersion;
            string result = Path.Combine(outputPath, targetPlatform.ToString(), dir);
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
            string directoryName = outputPath.Substring(outputPath.LastIndexOf("\\") + 1);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                if (file.Name == directoryName || file.Extension == ".manifest")
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
        private static CatAssetManifest GenerateManifestFile(string outputPath, List<AssetBundleBuild> abBuildList, AssetBundleManifest unityManifest,int manifestVersion)
        {

            CatAssetManifest manifest = new CatAssetManifest();
            manifest.GameVersion = Application.version;
            manifest.ManifestVersion = manifestVersion;

            manifest.Bundles = new BundleManifestInfo[abBuildList.Count];
            for (int i = 0; i < abBuildList.Count; i++)
            {
                AssetBundleBuild abBulid = abBuildList[i];

                BundleManifestInfo abInfo = new BundleManifestInfo();
                manifest.Bundles[i] = abInfo;

                abInfo.BundleName = abBulid.assetBundleName;

                string fullPath = Path.Combine(outputPath, abInfo.BundleName);
                FileInfo fi = new FileInfo(fullPath);

                abInfo.Length = fi.Length;
                abInfo.Hash = unityManifest.GetAssetBundleHash(abInfo.BundleName);

                abInfo.IsScene = abBulid.assetNames[0].EndsWith(".unity");  //判断是否为场景ab

                abInfo.Group = AssetCollector.GetAssetBundleGroup(abInfo.BundleName);  //标记资源组

                abInfo.Assets = new AssetManifestInfo[abBulid.assetNames.Length];
                for (int j = 0; j < abBulid.assetNames.Length; j++)
                {
                    AssetManifestInfo assetInfo = new AssetManifestInfo();
                    abInfo.Assets[j] = assetInfo;

                    assetInfo.AssetName = abBulid.assetNames[j];
                    assetInfo.Dependencies = PkgUtil.GetDependencies(assetInfo.AssetName,false);  //依赖列表不进行递归记录 因为加载的时候会对依赖进行递归加载
                }

               
            }

            //写入清单文件json
            string json = CatJson.JsonParser.ToJson(manifest);
            using (StreamWriter sw = new StreamWriter(Path.Combine(outputPath,Util.ManifestFileName)))
            {
                sw.Write(json);
            }

            return manifest;
            
        }
    
        /// <summary>
        /// 修改资源清单版本号
        /// </summary>
        private static void ChangeManifestVersion()
        {
            //自增
            PkgUtil.PkgCfg.ManifestVersion++;
        }
    
        /// <summary>
        /// 将资源复制到StreamingAssets下
        /// </summary>
        private static void CopyToStreamingAssets(string outputPath, string copyGroup,CatAssetManifest manifest)
        {
            //要复制的资源组的Set
            HashSet<string> copyGroupSet = null;
            if (!string.IsNullOrEmpty(copyGroup))
            {
                copyGroupSet = new HashSet<string>(copyGroup.Split(';'));
            }

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

            string manifestFileName = Util.ManifestFileName;

            foreach (FileInfo item in outputDirInfo.GetFiles())
            {
                if (item.Name == manifestFileName)
                {
                    //跳过资源清单文件
                    continue;
                }

                if (copyGroupSet != null
                    && !copyGroup.Contains(AssetCollector.GetAssetBundleGroup(item.Name)) 
                    )
                {
                    //跳过并非指定要复制的资源组的资源文件
                    continue;
                }

                item.CopyTo(Util.GetReadOnlyPath(item.Name));
            }

           

            if (copyGroupSet != null)
            {
                //根据要复制的资源组修改资源清单
                List<BundleManifestInfo> abInfoList = new List<BundleManifestInfo>();
                foreach (BundleManifestInfo abInfo in manifest.Bundles)
                {
                    if (copyGroupSet.Contains(abInfo.Group))
                    {
                        abInfoList.Add(abInfo);
                    }
                }
                manifest.Bundles = abInfoList.ToArray();
            }

            //生成仅包含被复制的资源组的资源清单文件到StreamingAssets下
            string json = CatJson.JsonParser.ToJson(manifest);
            using (StreamWriter sw = new StreamWriter(Util.GetReadOnlyPath(manifestFileName)))
            {
                sw.Write(json);
            }

            AssetDatabase.Refresh();
            Debug.Log("已将资源复制到StreamingAssets目录下");
        }
    }

}
