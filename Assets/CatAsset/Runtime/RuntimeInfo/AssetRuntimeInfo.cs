using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源运行时信息
    /// </summary>
    public class AssetRuntimeInfo : IComparable<AssetRuntimeInfo>, IEquatable<AssetRuntimeInfo>
    {
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
        /// 引用了此资源的资源集合
        /// </summary>
        public HashSet<AssetRuntimeInfo> RefAssets { get; } = new HashSet<AssetRuntimeInfo>();

        /// <summary>
        /// 是否可卸载
        /// </summary>
        public bool CanUnload()
        {
            return RefCount == 0;
        }
        
        
        public int CompareTo(AssetRuntimeInfo other)
        {
            return AssetManifest.CompareTo(other.AssetManifest);
        }

        public bool Equals(AssetRuntimeInfo other)
        {
            return BundleManifest.Equals(other.BundleManifest) && AssetManifest.Equals(other.AssetManifest);
        }

        public override string ToString()
        {
            return AssetManifest.ToString();
        }
    }
}