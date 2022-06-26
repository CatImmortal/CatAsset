using System;
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
        /// 资源类型名
        /// </summary>
        public string TypeName;

        private Type type;
        /// <summary>
        /// 资源类型
        /// </summary>
        public Type Type => type ??= AssetDatabase.GetMainAssetTypeAtPath(Name);

        public AssetBuildInfo(string name)
        {
            Name = name;
            TypeName = Type.Name;
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