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
        /// Bundle名
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length;

        /// <summary>
        /// 文件Hash
        /// </summary>
        public Hash128 Hash;

        /// <summary>
        /// 是否为场景的Bundle
        /// </summary>
        public bool IsScene;

        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

        /// <summary>
        /// 所有Asset清单信息
        /// </summary>
        public AssetManifestInfo[] Assets;

        public bool Equals(BundleManifestInfo other)
        {
            return BundleName == other.BundleName && Length == other.Length && Hash == other.Hash && Group == other.Group;
        }
    }
}

