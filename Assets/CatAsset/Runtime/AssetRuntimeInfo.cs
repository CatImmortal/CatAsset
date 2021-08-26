using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// Asset运行时信息
    /// </summary>
    public class AssetRuntimeInfo
    {
        public string AssetBundleName;

        public AssetManifestInfo ManifestInfo;

        public Object Asset;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int UseCount;
    }
}

