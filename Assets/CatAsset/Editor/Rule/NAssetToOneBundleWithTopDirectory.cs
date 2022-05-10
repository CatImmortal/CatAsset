using System.Collections.Generic;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有一级子目录各自使用NAssetToOneBundle规则进行构建
    /// </summary>
    public class NAssetToOneBundleWithTopDirectory : IBundleBuildRule
    {
        public List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory)
        {
            throw new System.NotImplementedException();
        }
    }
}