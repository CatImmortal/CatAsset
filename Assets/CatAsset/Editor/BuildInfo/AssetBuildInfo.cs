using System;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源构建信息
    /// </summary>
    [Serializable]
    public class AssetBuildInfo : IComparable<AssetBuildInfo>,IEquatable<AssetBuildInfo>
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public readonly string AssetName;

        public AssetBuildInfo(string assetName)
        {
            AssetName = assetName;
        }

        public override string ToString()
        {
            return AssetName;
        }
        
        public int CompareTo(AssetBuildInfo other)
        {
            return AssetName.CompareTo(other.AssetName);
        }

        public bool Equals(AssetBuildInfo other)
        {
            return AssetName.Equals(other.AssetName);
        }

        public override int GetHashCode()
        {
            return AssetName.GetHashCode();
        }
    }

}
