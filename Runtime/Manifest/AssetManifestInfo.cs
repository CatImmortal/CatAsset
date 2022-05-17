using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 资源清单信息
    /// </summary>
    public class AssetManifestInfo
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string AssetName;

        /// <summary>
        /// 依赖资源名列表
        /// </summary>
        public List<string> Dependencies;
    }

}
