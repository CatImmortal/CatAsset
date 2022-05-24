using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 构建管线
    /// </summary>
    public static class BuildPipeline
    {
        /// <summary>
        /// 构建资源包
        /// </summary>
        public static TaskResult BuildBundles(BundleBuildConfigSO bundleBuildConfig, BuildTarget targetPlatform)
        {
            BundleBuildConfigParam bundleBuildConfigParam = new BundleBuildConfigParam()
            {
                Config = bundleBuildConfig,
                TargetPlatform = targetPlatform,
            };
            
            BundleBuildsParam bundleBuildsParam = new BundleBuildsParam()
            {
                AssetBundleBuilds = bundleBuildConfig.GetAssetBundleBuilds(),
                NormalBundleBuilds = bundleBuildConfig.GetNormalBundleBuilds(),
                RawBundleBuilds = bundleBuildConfig.GetRawBundleBuilds(),
            };
          
            
            //注入构建管线参数
            BuildPipelineRunner.InjectParam(bundleBuildConfigParam);
            BuildPipelineRunner.InjectParam(bundleBuildsParam);

            //创建任务列表
            List<IBuildPipelineTask> tasks = new List<IBuildPipelineTask>()
            {
                new CreateOutputDirectoryTask(),
                new BuildAssetBundleTask(),
                new DeleteUnityManifestFileTask(),
                new BuildRawBundleTask(),
                new CreateManifestTask(),
                new WriteManifestFileTask(),
            };
            
            if (bundleBuildConfig.IsCopyToReadOnlyPath && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //需要复制资源包到只读目录下
                tasks.Add(new CopyToReadOnlyDirectoryTask());
                tasks.Add(new WriteManifestFileTask());
            }
            
            //运行构建管线任务
            return BuildPipelineRunner.Run(tasks);
        }

        /// <summary>
        /// 构建原生资源包
        /// </summary>
        public static void BuildRawBundles(BundleBuildConfigSO bundleBuildConfig,
            BuildTarget targetPlatform)
        {
            List<BundleBuildInfo> rawBundleBuilds = bundleBuildConfig.GetRawBundleBuilds();
            CatAssetManifest rawManifest = null;
            CatAssetManifest mainManifest = null;
            CatAssetManifest mergedManifest = null;
            
            //获取完整原生资源包构建输出目录
            string fullOutputPath = GetFullOutputPath(bundleBuildConfig.OutputPath, targetPlatform,
                bundleBuildConfig.ManifestVersion);
            Util.CreateEmptyDirectory(fullOutputPath);

            //构建原生资源包
            BuildRawBundles(fullOutputPath, rawBundleBuilds);

            //创建仅包含原生资源包的资源清单文件
            rawManifest = CreateManifest(fullOutputPath, bundleBuildConfig.ManifestVersion,
                new List<BundleBuildInfo>(), rawBundleBuilds, null);
            
            //获取主资源清单(将清单版本号-1)
            string mainOutputPath = GetFullOutputPath(bundleBuildConfig.OutputPath, targetPlatform,
                bundleBuildConfig.ManifestVersion - 1);
            string mainManifestPath = Path.Combine(mainOutputPath, Util.ManifestFileName);
            
            if (File.Exists(mainManifestPath))
            {
               string json = File.ReadAllText(mainManifestPath);
               mainManifest = CatJson.JsonParser.ParseJson<CatAssetManifest>(json);
            }
            
            //合并资源清单与资源包
            mergedManifest = MergeManifestAndBundles(mainOutputPath,fullOutputPath, mainManifest, rawManifest);
            
            //写入合并后的资源清单文件到构建输出目录下
            WriteManifestFile(fullOutputPath, mergedManifest);
            
            if (bundleBuildConfig.IsCopyToReadOnlyPath && bundleBuildConfig.TargetPlatforms.Count == 1)
            {
                //复制指定资源组的资源到只读目录下
                CopyToReadOnlyPath(fullOutputPath, bundleBuildConfig.CopyGroup, mergedManifest);
            }
        }

        /// <summary>
        /// 获取完整资源包构建输出目录
        /// </summary>
        public static string GetFullOutputPath(string outputPath, BuildTarget targetPlatform, int manifestVersion)
        {
            string dir = Application.version + "_" + manifestVersion;
            string result = Path.Combine(outputPath, targetPlatform.ToString(), dir);
            return result;
        }



        /// <summary>
        /// 构建原生资源包
        /// </summary>
        private static void BuildRawBundles(string outputPath, List<BundleBuildInfo> rawBundleBuilds)
        {
            foreach (BundleBuildInfo rawBundleBuildInfo in rawBundleBuilds)
            {
                string rawAssetName = rawBundleBuildInfo.Assets[0].AssetName;
                string fullDirectory = Path.Combine(outputPath, rawBundleBuildInfo.DirectoryName.ToLower());
                if (!Directory.Exists(fullDirectory))
                {
                    Directory.CreateDirectory(fullDirectory);
                }

                string targetFileName = Path.Combine(outputPath, rawBundleBuildInfo.RelativePath);
                File.Copy(rawAssetName, targetFileName); //直接将原生资源复制过去
            }
        }

        /// <summary>
        /// 生成资源清单
        /// </summary>
        private static CatAssetManifest CreateManifest(string outputPath, int manifestVersion,
            List<BundleBuildInfo> bundleBuilds, List<BundleBuildInfo> rawBundleBuilds,
            AssetBundleManifest unityManifest)
        {
            CatAssetManifest manifest = new CatAssetManifest
            {
                GameVersion = Application.version,
                ManifestVersion = manifestVersion,
            };

            //创建普通资源包的清单信息
            foreach (BundleBuildInfo bundleBuildInfo in bundleBuilds)
            {
                BundleManifestInfo bundleManifestInfo = new BundleManifestInfo()
                {
                    RelativePath = bundleBuildInfo.RelativePath,
                    Directory = bundleBuildInfo.DirectoryName,
                    BundleName = bundleBuildInfo.BundleName,
                    Group = bundleBuildInfo.Group,
                    IsRaw = false,
                };
                manifest.Bundles.Add(bundleManifestInfo);

                bundleManifestInfo.IsScene = bundleBuildInfo.Assets[0].AssetName.EndsWith(".unity");

                string fullPath = Path.Combine(outputPath, bundleBuildInfo.RelativePath);
                FileInfo fi = new FileInfo(fullPath);
                bundleManifestInfo.Length = fi.Length;

                bundleManifestInfo.Hash = unityManifest.GetAssetBundleHash(bundleBuildInfo.RelativePath);

                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                    {
                        AssetName = assetBuildInfo.AssetName,
                    };
                    bundleManifestInfo.Assets.Add(assetManifestInfo);

                    //依赖列表不进行递归记录 因为加载的时候会对依赖进行递归加载
                    assetManifestInfo.Dependencies = Util.GetDependencies(assetManifestInfo.AssetName, false);
                }
            }

            //创建原生资源包的清单信息
            foreach (BundleBuildInfo bundleBuildInfo in rawBundleBuilds)
            {
                BundleManifestInfo bundleManifestInfo = new BundleManifestInfo()
                {
                    RelativePath = bundleBuildInfo.RelativePath,
                    Directory = bundleBuildInfo.DirectoryName,
                    BundleName = bundleBuildInfo.BundleName,
                    Group = bundleBuildInfo.Group,
                    IsRaw = true,
                    IsScene = false,
                };
                manifest.Bundles.Add(bundleManifestInfo);

                string fullPath = Path.Combine(outputPath, bundleBuildInfo.RelativePath);
                byte[] bytes = File.ReadAllBytes(fullPath);
                bundleManifestInfo.Length = bytes.Length;

                bundleManifestInfo.Hash = Hash128.Compute(bytes);

                AssetManifestInfo assetManifestInfo = new AssetManifestInfo()
                {
                    AssetName = bundleBuildInfo.Assets[0].AssetName,
                };
                bundleManifestInfo.Assets.Add(assetManifestInfo);
            }

            manifest.Bundles.Sort();

            return manifest;
        }

        /// <summary>
        /// 合并资源清单与资源包
        /// </summary>
        private static CatAssetManifest MergeManifestAndBundles(string mainOutputPath,string outputPath, CatAssetManifest main, CatAssetManifest raw)
        {
            if (main == null)
            {
                return raw;
            }
            
            foreach (BundleManifestInfo bundleManifestInfo in main.Bundles)
            {
                if (!bundleManifestInfo.IsRaw)
                {
                    //合并原生资源包
                    FileInfo fi = new FileInfo(Path.Combine(mainOutputPath, bundleManifestInfo.RelativePath));

                    string fullPath = Path.Combine(outputPath, bundleManifestInfo.RelativePath);
                    string fullDirectory =  Path.Combine(outputPath, bundleManifestInfo.Directory.ToLower());
                    
                    if (!Directory.Exists(fullDirectory))
                    {
                        //目录不存在则创建
                        Directory.CreateDirectory(fullDirectory);
                    }

                    fi.CopyTo(fullPath);
                    
                    //合并资源清单记录
                    raw.Bundles.Add(bundleManifestInfo);
                }
            }

            raw.Bundles.Sort();
            return raw;
        }
        
        /// <summary>
        /// 写入资源清单文件
        /// </summary>
        private static void WriteManifestFile(string outputPath, CatAssetManifest manifest)
        {
            //写入清单文件json
            string json = CatJson.JsonParser.ToJson(manifest);
            using (StreamWriter sw = new StreamWriter(Path.Combine(outputPath, Util.ManifestFileName)))
            {
                sw.Write(json);
            }
        }

        /// <summary>
        /// 将指定资源组的资源复制到只读目录下
        /// </summary>
        private static void CopyToReadOnlyPath(string outputPath, string copyGroup, CatAssetManifest manifest)
        {
            //要复制的资源组的Set
            HashSet<string> copyGroupSet = null;
            if (!string.IsNullOrEmpty(copyGroup))
            {
                copyGroupSet = new HashSet<string>(copyGroup.Split(';'));
            }

            Util.CreateEmptyDirectory(Application.streamingAssetsPath);

            List<BundleManifestInfo> copiedBundles = new List<BundleManifestInfo>();

            //复制指定组的资源文件
            foreach (BundleManifestInfo bundleManifestInfo in manifest.Bundles)
            {
                if (copyGroupSet != null)
                {
                    if (!copyGroupSet.Contains(bundleManifestInfo.Group))
                    {
                        //跳过并非指定资源组的资源文件
                        continue;
                    }
                }

                FileInfo fi = new FileInfo(Path.Combine(outputPath, bundleManifestInfo.RelativePath));

                string fullPath = CatAsset.Runtime.Util.GetReadOnlyPath(bundleManifestInfo.RelativePath);
                string fullDirectory = CatAsset.Runtime.Util.GetReadOnlyPath(bundleManifestInfo.Directory.ToLower());
                if (!Directory.Exists(fullDirectory))
                {
                    //StreamingAssets下的目录不存在则创建
                    Directory.CreateDirectory(fullDirectory);
                }

                fi.CopyTo(fullPath);

                copiedBundles.Add(bundleManifestInfo);
            }

            //根据复制过去的资源包修改资源清单
            manifest.Bundles = copiedBundles;

            //写入仅包含被复制的资源包的资源清单文件到只读区下
            WriteManifestFile(Application.streamingAssetsPath, manifest);

            AssetDatabase.Refresh();
        }
    }
}