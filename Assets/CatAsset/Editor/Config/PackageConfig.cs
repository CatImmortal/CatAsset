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
        public List<BuildTarget> targetPlatforms;

        /// <summary>
        /// 打包设置
        /// </summary>
        public BuildAssetBundleOptions options;

        /// <summary>
        /// 打包输出目录
        /// </summary>
        public string outputPath;

        [MenuItem("CatAsset/创建打包配置文件")]
        private static void CreateConfig()
        {
            Util.CreateConfigAsset<PackageConfig>();
        }

        private void Awake()
        {
            targetPlatforms = new List<BuildTarget>();
            targetPlatforms.Add(BuildTarget.StandaloneWindows);

            options = BuildAssetBundleOptions.ChunkBasedCompression;

            outputPath = Directory.GetCurrentDirectory() + "\\AssetBundleOutput";
        }
    }
}

