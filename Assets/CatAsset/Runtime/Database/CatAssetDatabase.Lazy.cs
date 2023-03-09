using System.Collections.Generic;

namespace CatAsset.Runtime
{
    public static partial class CatAssetDatabase
    {
        /// <summary>
        /// 用于延迟初始化的资源包信息
        /// </summary>
        internal struct LazyBundleInfo
        {
            public BundleManifestInfo Manifest;
            public BundleRuntimeInfo.State State;

            public override string ToString()
            {
                return Manifest.ToString();
            }
        }
        
        /// <summary>
        /// 用于延迟初始化的资源信息
        /// </summary>
        private struct LazyAssetInfo
        {
            public BundleManifestInfo BundleManifestInfo;
            public AssetManifestInfo AssetManifestInfo;

            public override string ToString()
            {
                return AssetManifestInfo.ToString();
            }
        }

        /// <summary>
        /// 资源包名 -> 用于延迟初始化的资源包信息
        /// </summary>
        private static Dictionary<string, LazyBundleInfo> lazyBundleInfoDict = new Dictionary<string, LazyBundleInfo>();
        
        /// <summary>
        /// 资源名 -> 用于延迟初始化的资源信息
        /// </summary>
        private static Dictionary<string, LazyAssetInfo> lazyAssetInfoDict = new Dictionary<string, LazyAssetInfo>();
        
        /// <summary>
        /// 获取所有用于延迟初始化的资源包信息
        /// </summary>
        internal static Dictionary<string, LazyBundleInfo> GetAllLazyBundleInfo()
        {
            return lazyBundleInfoDict;
        }
    }
}