using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// Bundle清单信息
    /// </summary>
    [Serializable]
    public class BundleManifestInfo : IComparable<BundleManifestInfo>,IEquatable<BundleManifestInfo>
    {
        private string relativePath;
        
        /// <summary>
        /// 相对路径
        /// </summary>
        public string RelativePath{
            get
            {
                if (relativePath == null)
                {
                    relativePath = RuntimeUtil.GetRegularPath(Path.Combine(Directory, BundleName));
                }
                return relativePath;
            }
        }
        
        /// <summary>
        /// 目录名
        /// </summary>
        public string Directory;
        
        /// <summary>
        /// 资源包名
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

        /// <summary>
        /// 是否为原生资源包
        /// </summary>
        public bool IsRaw;
        
        /// <summary>
        /// 是否为场景资源包
        /// </summary>
        public bool IsScene;

        /// <summary>
        /// 文件长度
        /// </summary>
        public ulong Length;
        
        /// <summary>
        /// 文件MD5
        /// </summary>
        public string MD5;

        /// <summary>
        /// 文件Hash值
        /// </summary>
        public string Hash;

        /// <summary>
        /// 是否依赖内置Shader资源包
        /// </summary>
        public bool IsDependencyBuiltInShaderBundle;
        
        /// <summary>
        /// 资源清单信息列表
        /// </summary>
        public List<AssetManifestInfo> Assets = new List<AssetManifestInfo>();

        public int CompareTo(BundleManifestInfo other)
        {
            return RelativePath.CompareTo(other.RelativePath);
        }
        
        public override string ToString()
        {
            return RelativePath;
        }

        public bool Equals(BundleManifestInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return relativePath == other.relativePath && Length == other.Length && MD5 == other.MD5;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BundleManifestInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (relativePath != null ? relativePath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Length.GetHashCode();
                hashCode = (hashCode * 397) ^ (MD5 != null ? MD5.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}

