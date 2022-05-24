using System.Collections.Generic;
using UnityEditor;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建信息参数
    /// </summary>
    public class BundleBuildsParam : IBuildPipelineParam
    {
        public List<AssetBundleBuild> AssetBundleBuilds;
        public List<BundleBuildInfo> NormalBundleBuilds;
        public List<BundleBuildInfo> RawBundleBuilds;
    }
}