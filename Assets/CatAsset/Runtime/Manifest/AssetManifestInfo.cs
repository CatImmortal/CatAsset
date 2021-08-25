using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// Asset清单信息
    /// </summary>
    public class AssetManifestInfo
    {
        /// <summary>
        /// Asset名
        /// </summary>
        public string AssetName;

        /// <summary>
        /// 依赖的所有Asset
        /// </summary>
        public string[] Dependencies;
    }

}
