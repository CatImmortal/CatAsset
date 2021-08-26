using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// AssetBundle运行时信息
    /// </summary>
    public class AssetBundleRuntimeInfo
    {
        public AssetBundleManifestInfo ManifestInfo;
        public AssetBundle AssetBundle;

        private HashSet<string> usedAsset;

        /// <summary>
        /// 当前使用中的Asset
        /// </summary>
        public HashSet<string> UsedAsset
        {
            get
            {
                if (usedAsset == null)
                {
                    usedAsset = new HashSet<string>();
                }

                return usedAsset;
            }
        }
    }
}

