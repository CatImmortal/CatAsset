using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// AssetBundle清单信息
    /// </summary>
    public class AssetBundleManifestInfo
    {
        /// <summary>
        /// AssetBundle名
        /// </summary>
        public string AssetBundleName;

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length;

        /// <summary>
        /// 文件Hash
        /// </summary>
        public int Hash;

        /// <summary>
        /// 是否为场景的AssetBundle包
        /// </summary>
        public bool IsScene;

        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

        /// <summary>
        /// AssetBundle中的所有Asset清单信息
        /// </summary>
        public AssetManifestInfo[] Assets;

        public bool Equals(AssetBundleManifestInfo abInfo)
        {
            return AssetBundleName == abInfo.AssetBundleName && Length == abInfo.Length && Hash == abInfo.Hash;
        }
    }
}

