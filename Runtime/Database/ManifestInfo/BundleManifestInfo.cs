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
                    if (IsAppendMD5)
                    {
                        //附加了MD5值到资源包文件名中
                        string[] nameArray = BundleName.Split('.');
                        string md5BundleName =   $"{nameArray[0]}_{MD5}.{nameArray[1]}";
                        relativePath = RuntimeUtil.GetRegularPath(Path.Combine(Directory,md5BundleName));
                    }
                    else
                    {
                        relativePath = BundleIdentifyName;
                    }
                }
                return relativePath;
            }
        }

        /// <summary>
        /// 目录名
        /// </summary>
        [NonSerialized]
        public string Directory;

        public int DirectoryNodeID;

        /// <summary>
        /// 资源包名
        /// </summary>
        [NonSerialized]
        public string BundleName;

        public int BundleNameNodeID;

        private string bundleIdentifyName;
        /// <summary>
        /// 资源包标识名
        /// </summary>
        public string BundleIdentifyName
        {
            get
            {
                if (bundleIdentifyName == null)
                {
                    bundleIdentifyName = RuntimeUtil.GetRegularPath(Path.Combine(Directory,BundleName));
                }

                return bundleIdentifyName;
            }
        }

        /// <summary>
        /// 资源组
        /// </summary>
        [NonSerialized]
        public string Group;

        public int GroupNodeID;
        
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
        /// 是否附加MD5值到资源包名中
        /// </summary>
        public bool IsAppendMD5;

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
        [NonSerialized]
        public List<AssetManifestInfo> Assets;

        /// <summary>
        /// 资源清单信息ID列表
        /// </summary>
        public List<int> AssetIDs;

        public int CompareTo(BundleManifestInfo other)
        {
            return BundleIdentifyName.CompareTo(other.BundleIdentifyName);
        }

        public override string ToString()
        {
            return BundleIdentifyName;
        }

        public bool Equals(BundleManifestInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BundleIdentifyName == other.BundleIdentifyName && Length == other.Length && MD5 == other.MD5;
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
                var hashCode = (BundleIdentifyName != null ? BundleIdentifyName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Length.GetHashCode();
                hashCode = (hashCode * 397) ^ (MD5 != null ? MD5.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// 序列化为二进制数据
        /// </summary>
        public static byte[] Serialize()
        {
            return null;
        }

        /// <summary>
        /// 从二进制数据反序列化
        /// </summary>
        public BundleManifestInfo Deserialize(byte[] bytes)
        {
            return null;
        }
    }
}

