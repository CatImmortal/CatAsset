using System.Collections.Generic;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有资源分别构建为一个原生资源包
    /// </summary>
    public class NAssetToNRawBundle : NAssetToNBundle
    {
        /// <inheritdoc />
        public override bool IsRaw => true;
    }
}