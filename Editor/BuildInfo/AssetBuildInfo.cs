using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源构建信息
    /// </summary>
    [Serializable]
    public class AssetBuildInfo
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string AssetName;

        public AssetBuildInfo(string assetName)
        {
            AssetName = assetName;
        }
    }

}
