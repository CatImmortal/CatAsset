using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建信息参数
    /// </summary>
    public interface IBundleBuildInfoParam : IContextObject
    {
        public List<AssetBundleBuild> AssetBundleBuilds { get; }
        public List<BundleBuildInfo> NormalBundleBuilds { get; }
        public List<BundleBuildInfo> RawBundleBuilds { get; }
    }
}