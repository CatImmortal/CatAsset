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
        /// <summary>
        /// 所属的AssetBundle的名称
        /// </summary>
        public string AssetBundleName;

        /// <summary>
        /// Asset清单信息
        /// </summary>
        public AssetManifestInfo ManifestInfo;

        /// <summary>
        /// 已加载的Asset实例
        /// </summary>
        public Object Asset;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount;
    }
}

