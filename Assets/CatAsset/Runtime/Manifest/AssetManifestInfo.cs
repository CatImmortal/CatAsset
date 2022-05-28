using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源清单信息
    /// </summary>
    public class AssetManifestInfo : IComparable<AssetManifestInfo>,IEquatable<AssetManifestInfo>
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string AssetName;

        /// <summary>
        /// 资源类型
        /// </summary>
        public Type AssetType;
        
        /// <summary>
        /// 依赖资源名列表
        /// </summary>
        public List<string> Dependencies;

        public int CompareTo(AssetManifestInfo other)
        {
            return AssetName.CompareTo(other.AssetName);
        }

        public bool Equals(AssetManifestInfo other)
        {
            return AssetName.Equals(other.AssetName);
        }

        public override string ToString()
        {
            return AssetName;
        }
    }

}
