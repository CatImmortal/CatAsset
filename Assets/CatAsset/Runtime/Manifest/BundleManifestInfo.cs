using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// Bundle清单信息
    /// </summary>
    public class BundleManifestInfo
    {
        /// <summary>
        /// 相对路径
        /// </summary>
        public string RelativePath;
        
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
        /// 文件Hash
        /// </summary>
        public Hash128 Hash;

        /// <summary>
        /// 资源清单信息列表
        /// </summary>
        public List<AssetManifestInfo> Assets = new List<AssetManifestInfo>();

        public bool Equals(BundleManifestInfo other)
        {
            return RelativePath == other.RelativePath && Length == other.Length && Hash == other.Hash && Group == other.Group;
        }
    }
}

