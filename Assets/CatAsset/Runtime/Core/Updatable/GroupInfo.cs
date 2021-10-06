using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 资源组信息
    /// </summary>
    public class GroupInfo
    {
        /// <summary>
        /// 资源组名
        /// </summary>
        public string GroupName;

        /// <summary>
        /// 此资源组的所有远端AssetBundle名
        /// </summary>
        public List<string> remoteAssetBunldes = new List<string>();

        /// <summary>
        /// 此资源组的所有远端AssetBundle数量
        /// </summary>
        public int remoteCount;

        /// <summary>
        /// 此资源组的所有远端AssetBundle长度
        /// </summary>
        public long remoteLength;


        /// <summary>
        /// 此资源组的所有本地AssetBundle名
        /// </summary>
        public List<string> localAssetBundles = new List<string>();

        /// <summary>
        /// 此资源组的所有本地AssetBundle数量
        /// </summary>
        public int localCount;

        /// <summary>
        /// 此资源组的所有本地AssetBundle长度
        /// </summary>
        public long localLength;
    }
}

