using System;
using System.IO;
using UnityEditor;

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
        public string Name;

        /// <summary>
        /// 所在资源包的相对路径
        /// </summary>
        public string BundleRelativePath;

        private Type type;
        /// <summary>
        /// 资源类型
        /// </summary>
        public Type Type => type ??= AssetDatabase.GetMainAssetTypeAtPath(Name);

        /// <summary>
        /// 资源文件长度
        /// </summary>
        public long Length;
        
        public AssetBuildInfo(string name,string bundleRelativePath)
        {
            Name = name;
            BundleRelativePath = bundleRelativePath;
            Length = new FileInfo(name).Length;
        }

        public override string ToString()
        {
            return Name;
        }
        
        public int CompareTo(AssetBuildInfo other)
        {
            return Name.CompareTo(other.Name);
        }

        public bool Equals(AssetBuildInfo other)
        {
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

}
