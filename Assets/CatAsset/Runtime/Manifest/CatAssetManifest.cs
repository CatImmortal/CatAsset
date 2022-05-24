using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
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
        /// 资源包清单信息列表
        /// </summary>
        public List<BundleManifestInfo> Bundles = new List<BundleManifestInfo>();
    }
}

