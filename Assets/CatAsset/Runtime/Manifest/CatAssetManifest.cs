using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// CatAsset资源清单
    /// </summary>
    public class CatAssetManifest
    {
        /// <summary>
        /// 游戏版本号
        /// </summary>
        public string GameVersion;

        /// <summary>
        /// 清单版本号
        /// </summary>
        public int ManifestVersion;

        /// <summary>
        /// 所有AssetBundle
        /// </summary>
        public AssetBundleManifestInfo[] AssetBundles;
    }
}

