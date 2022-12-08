using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源构建信息
    /// </summary>
    [Serializable]
    public class AssetBuildInfo : IComparable<AssetBuildInfo>,IEquatable<AssetBuildInfo>
    {
        private static MethodInfo getStorageMemorySizeLongMethod =
            typeof(EditorWindow).Assembly.GetType("UnityEditor.TextureUtil").GetMethod("GetStorageMemorySizeLong",
                BindingFlags.Static | BindingFlags.Public);

        private static object[] paramCache = new object[1];
        
        /// <summary>
        /// 资源名
        /// </summary>
        public string Name;

        private Type type;

        /// <summary>
        /// 资源类型
        /// </summary>
        public Type Type => type ?? (type = AssetDatabase.GetMainAssetTypeAtPath(Name));

        /// <summary>
        /// 资源文件长度
        /// </summary>
        public ulong Length;
        
        public AssetBuildInfo(string name)
        {
            Name = name;
            if (Type == typeof(Texture2D))
            {
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
                paramCache[0] = texture2D;
                long size = (long)getStorageMemorySizeLongMethod.Invoke(null, paramCache);
                Length = (ulong)size;
            }
            else
            {
                Length = (ulong)new FileInfo(name).Length;
            }
         
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
