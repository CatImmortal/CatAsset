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
        /// AssetBundle中的所有Asset清单信息
        /// </summary>
        public AssetManifestInfo[] Assets;

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length;

        /// <summary>
        /// 文件Hash
        /// </summary>
        public Hash128 Hash;
    }
}

