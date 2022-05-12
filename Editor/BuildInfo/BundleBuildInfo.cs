using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建信息
    /// </summary>
    [Serializable]
    public class BundleBuildInfo : IComparable<BundleBuildInfo>,IEquatable<BundleBuildInfo>
    {
        /// <summary>
        /// 相对路径
        /// </summary>
        public string RelativePath;
        
        /// <summary>
        /// 目录名
        /// </summary>
        public string DirectoryName;
        
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
        /// 资源构建信息列表
        /// </summary>
        public List<AssetBuildInfo> Assets = new List<AssetBuildInfo>();
        
        
        public BundleBuildInfo(string directoryName, string bundleName,string group,bool isRaw)
        {
            DirectoryName = directoryName;
            BundleName = bundleName;
            Group = group;
            IsRaw = isRaw;
            
            if (!string.IsNullOrEmpty(DirectoryName))
            {
                RelativePath = $"{DirectoryName}/{BundleName}";
            }
            else
            {
                RelativePath = BundleName;
            }
        }

        public override string ToString()
        {
            return RelativePath;
        }
        
        public int CompareTo(BundleBuildInfo other)
        {
            return RelativePath.CompareTo(other.RelativePath);
        }
        
        public bool Equals(BundleBuildInfo other)
        {
            return RelativePath.Equals(other.RelativePath);
        }

        public override int GetHashCode()
        {
            return RelativePath.GetHashCode();
        }
    }

}
