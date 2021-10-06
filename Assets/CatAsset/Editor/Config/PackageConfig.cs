using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 打包配置
    /// </summary>
    public class PackageConfig : ScriptableObject
    {
        /// <summary>
        /// 资源清单版本号
        /// </summary>
        public int ManifestVersion;

        /// <summary>
        /// 要打包的平台
        /// </summary>
        public List<BuildTarget> TargetPlatforms;

        /// <summary>
        /// 打包设置
        /// </summary>
        public BuildAssetBundleOptions Options;

        /// <summary>
        /// 打包输出目录
        /// </summary>
        public string OutputPath;

        /// <summary>
        /// 是否进行冗余分析
        /// </summary>
        public bool IsAnalyzeRedundancy;

        /// <summary>
        /// 打包平台只有1个时，打包后是否将资源复制到StreamingAssets目录下
        /// </summary>
        public bool IsCopyToStreamingAssets;

        /// <summary>
        /// 要复制到StreamingAssets目录下的资源组，以分号分隔
        /// </summary>
        public string CopyGroup;

        [MenuItem("CatAsset/创建打包配置文件")]
        private static void CreateConfig()
        {
            PackageConfig cfg = PkgUtil.CreateConfigAsset<PackageConfig>();

            if (cfg != null)
            {
                cfg.TargetPlatforms = new List<BuildTarget>();

                cfg.TargetPlatforms.Add(BuildTarget.StandaloneWindows);

                cfg.IsAnalyzeRedundancy = true;

                cfg.Options = BuildAssetBundleOptions.ChunkBasedCompression
                    | BuildAssetBundleOptions.DisableWriteTypeTree
                    | BuildAssetBundleOptions.DisableLoadAssetByFileName
                    | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

                cfg.OutputPath = Path.Combine(Directory.GetCurrentDirectory(), "AssetBundleOutput");

                cfg.ManifestVersion = 1;

                cfg.IsCopyToStreamingAssets = true;

                cfg.CopyGroup = Util.DefaultGroup;

                EditorUtility.SetDirty(cfg);
            }
        }



        
    }
}

