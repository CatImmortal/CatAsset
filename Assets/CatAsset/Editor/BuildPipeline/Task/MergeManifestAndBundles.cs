using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 合并资源清单与资源包
    /// </summary>
    public class MergeManifestAndBundles : IBuildTask
    {
        [InjectContext(ContextUsage.In)]
        private IBundleBuildParameters buildParam;

        [InjectContext(ContextUsage.In)]
        private IBundleBuildConfigParam configParam;

        [InjectContext(ContextUsage.In)]
        private IManifestParam manifestParam;

        /// <inheritdoc />
        public int Version => 1;

        /// <inheritdoc />
        public ReturnCode Run()
        {
                BundleBuildConfigSO bundleBuildConfig = configParam.Config;
                BuildTarget targetPlatform = configParam.TargetPlatform;
                string directory = ((BundleBuildParameters) buildParam).OutputFolder;
                CatAssetManifest rawManifest = manifestParam.Manifest;

                //主资源清单
                int mainManifestVersion = bundleBuildConfig.ManifestVersion;
                CatAssetManifest mainManifest = null;
                string mainOutputPath = null;
                do
                {
                    //尝试获取前一个版本的主资源清单
                    //将清单版本号不断-1 直到0
                    mainManifestVersion--;
                    mainOutputPath = EditorUtil.GetFullOutputPath(bundleBuildConfig.OutputRootDirectory, targetPlatform,
                        bundleBuildConfig.ManifestVersion - 1);
                    string mainManifestPath = Path.Combine(mainOutputPath,CatAssetManifest.ManifestBinaryFileName);

                    if (File.Exists(mainManifestPath))
                    {
                        //存在前一个版本的主资源清单 读取
                        byte[] bytes = File.ReadAllBytes(mainManifestPath);
                        mainManifest = CatAssetManifest.DeserializeFromBinary(bytes);
                        break;
                    }

                } while (mainManifestVersion >= 0);

                if (mainManifest == null)
                {
                    //不存在前一个版本的主资源清单 意味着不需要合并
                    return ReturnCode.SuccessNotRun;
                }

                foreach (BundleManifestInfo bundleManifestInfo in mainManifest.Bundles)
                {
                    if (!bundleManifestInfo.IsRaw)
                    {
                        //合并资源包
                        FileInfo fi = new FileInfo(Path.Combine(mainOutputPath, bundleManifestInfo.RelativePath));

                        string fullPath = Path.Combine(directory, bundleManifestInfo.RelativePath);
                        string fullDirectory =  Path.Combine(directory, bundleManifestInfo.Directory);

                        if (!Directory.Exists(fullDirectory))
                        {
                            //目录不存在则创建
                            Directory.CreateDirectory(fullDirectory);
                        }

                        fi.CopyTo(fullPath);

                        //合并资源清单记录
                        rawManifest.Bundles.Add(bundleManifestInfo);
                    }
                }

                rawManifest.Bundles.Sort();

            return ReturnCode.Success;
        }


    }
}
