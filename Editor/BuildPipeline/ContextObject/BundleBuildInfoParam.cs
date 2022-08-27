using System.Collections.Generic;
using UnityEditor;

namespace CatAsset.Editor
{
    /// <inheritdoc />
    public class BundleBuildInfoParam : IBundleBuildInfoParam
    {

        /// <inheritdoc />
        public List<AssetBundleBuild> AssetBundleBuilds { get; }
        
        /// <inheritdoc />
        public List<BundleBuildInfo> NormalBundleBuilds { get; }
        
        /// <inheritdoc />
        public List<BundleBuildInfo> RawBundleBuilds { get; }

        public BundleBuildInfoParam(List<AssetBundleBuild> assetBundleBuilds, List<BundleBuildInfo> normalBundleBuilds, List<BundleBuildInfo> rawBundleBuilds)
        {
            AssetBundleBuilds = assetBundleBuilds;
            NormalBundleBuilds = normalBundleBuilds;
            RawBundleBuilds = rawBundleBuilds;
        }
    }
}