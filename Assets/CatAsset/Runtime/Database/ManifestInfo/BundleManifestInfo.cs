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
        public long Length;
        
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
        
        public bool Equals(BundleManifestInfo other)
        {
            return RelativePath.Equals(other.RelativePath)  && Length.Equals(other.Length) && MD5.Equals(other.MD5) && Group.Equals(other.Group);
        }

        public override string ToString()
        {
            return RelativePath;
        }
    }
}

