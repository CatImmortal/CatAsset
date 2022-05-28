using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源运行时信息
    /// </summary>
    public class AssetRuntimeInfo
    {
        private List<string> refAssetList;

        /// <summary>
        /// 所在资源包清单信息
        /// </summary>
        public BundleManifestInfo BundleManifest;

        /// <summary>
        /// 资源清单信息
        /// </summary>
        public AssetManifestInfo AssetManifest;

        /// <summary>
        /// 已加载的资源实例
        /// </summary>
        public object Asset;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount;

        /// <summary>
        /// 通过依赖加载引用了此资源的资源列表
        /// </summary>
        public List<string> RefAssetList{
            get
            {
                if (refAssetList == null)
                {
                    refAssetList = new List<string>();
                }
                
                return refAssetList;
            }
        }

        public override string ToString()
        {
            return AssetManifest.ToString();
        }
    }
}