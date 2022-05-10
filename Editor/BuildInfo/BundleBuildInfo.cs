using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建信息
    /// </summary>
    [Serializable]
    public class BundleBuildInfo
    {
        /// <summary>
        /// 资源包名
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

        /// <summary>
        /// 是否为原生资源包
        /// </summary>
        public bool IsRaw;
        
        /// <summary>
        /// 资源构建信息列表
        /// </summary>
        public List<AssetBuildInfo> Assets = new List<AssetBuildInfo>();

        public BundleBuildInfo(string bundleName,string group,bool isRaw)
        {
            BundleName = bundleName;
            Group = group;
            IsRaw = isRaw;
        }
    }

}
