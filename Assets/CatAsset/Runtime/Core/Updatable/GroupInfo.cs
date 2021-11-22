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
        /// 此资源组的所有远端Bundle名
        /// </summary>
        public List<string> remoteBunldes = new List<string>();

        /// <summary>
        /// 此资源组的所有远端Bundle数量
        /// </summary>
        public int remoteCount;

        /// <summary>
        /// 此资源组的所有远端Bundle长度
        /// </summary>
        public long remoteLength;


        /// <summary>
        /// 此资源组的所有本地Bundle名
        /// </summary>
        public List<string> localBundles = new List<string>();

        /// <summary>
        /// 此资源组的所有本地Bundle数量
        /// </summary>
        public int localCount;

        /// <summary>
        /// 此资源组的所有本地Bundle长度
        /// </summary>
        public long localLength;
    }
}

