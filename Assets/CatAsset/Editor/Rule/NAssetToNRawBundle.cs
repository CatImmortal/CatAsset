using System.Collections.Generic;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有资源分别构建为一个原生资源包
    /// </summary>
    public class NAssetToNRawBundle : IBundleBuildRule
    {
        public List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory)
        {
            throw new System.NotImplementedException();
        }
    }
}