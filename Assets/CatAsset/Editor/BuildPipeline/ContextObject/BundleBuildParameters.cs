using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// SBP用到的资源包构建参数
    /// </summary>
    public class BundleBuildParameters : UnityEditor.Build.Pipeline.BundleBuildParameters
    {

        /// <summary>
        /// 资源包相对路径 -> 资源包构建信息
        /// </summary>
        private Dictionary<string, BundleBuildInfo> bundleBuildInfos = new Dictionary<string, BundleBuildInfo>();

        public BundleBuildParameters(BuildTarget target, BuildTargetGroup group, string outputFolder) : base(target, group, outputFolder)
        {
            foreach (BundleBuildInfo bundleBuildInfo in BundleBuildConfigSO.Instance.Bundles)
            {
                bundleBuildInfos.Add(bundleBuildInfo.BundleIdentifyName,bundleBuildInfo);
            }
        }

        public override BuildCompression GetCompressionForIdentifier(string identifier)
        {
            if (!bundleBuildInfos.TryGetValue(identifier,out var bundleBuildInfo))
            {
                return BundleCompression;
            }
            
            BundleCompressOptions compressOption = bundleBuildInfo.CompressOption;

            switch (compressOption)
            {
                case BundleCompressOptions.Uncompressed:
                    return BuildCompression.Uncompressed;

                case BundleCompressOptions.LZ4:
                    return BuildCompression.LZ4;
                
                case BundleCompressOptions.LZMA:
                    return BuildCompression.LZMA;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}