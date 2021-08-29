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
        /// <summary>
        /// AssetBundle清单信息
        /// </summary>
        public AssetBundleManifestInfo ManifestInfo;

        /// <summary>
        /// 已加载的AssetBundle实例
        /// </summary>
        public AssetBundle AssetBundle;

        /// <summary>
        /// AssetBundle物理加载地址
        /// </summary>
        public string LoadPath;


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

