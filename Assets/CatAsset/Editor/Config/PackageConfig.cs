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
        /// 要打包的平台
        /// </summary>
        public List<BuildTarget> TargetPlatforms;

        /// <summary>
        /// 打包设置
        /// </summary>
        public BuildAssetBundleOptions Options;

        /// <summary>
        /// 是否进行冗余分析
        /// </summary>
        public bool IsAnalyzeRedundancy;

        /// <summary>
        /// 打包输出目录
        /// </summary>
        public string OutputPath;

        /// <summary>
        /// 资源清单版本号
        /// </summary>
        public int ManifestVersion;

        /// <summary>
        /// 打包平台只有1个时，打包后是否将资源复制到StreamingAssets目录下
        /// </summary>
        public bool IsCopyToStreamingAssets;

        [MenuItem("CatAsset/创建打包配置文件")]
        private static void CreateConfig()
        {
            Util.CreateConfigAsset<PackageConfig>();
        }

        private void Awake()
        {
            TargetPlatforms = new List<BuildTarget>();
            TargetPlatforms.Add(BuildTarget.StandaloneWindows);

            IsAnalyzeRedundancy = true;

            Options = BuildAssetBundleOptions.ChunkBasedCompression 
                | BuildAssetBundleOptions.DisableWriteTypeTree 
                | BuildAssetBundleOptions.DisableLoadAssetByFileName 
                | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

            OutputPath = Directory.GetCurrentDirectory() + "\\AssetBundleOutput";
            IsCopyToStreamingAssets = true;
        }

        
    }
}

