﻿using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源组信息
    /// </summary>
    public class GroupInfo
    {
        /// <summary>
        /// 默认资源组
        /// </summary>
        public const string DefaultGroup = "Base";

        /// <summary>
        /// 资源组名
        /// </summary>
        public string GroupName { get; internal set; }

        /// <summary>
        /// 此资源组的所有本地资源包
        /// </summary>
        private List<string> localBundles = new List<string>();

        /// <summary>
        /// 此资源组的所有本地资源包数量
        /// </summary>
        public int LocalCount => localBundles.Count;

        /// <summary>
        /// 此资源组的所有本地资源包长度
        /// </summary>
        public ulong LocalLength { get; internal set; }

        /// <summary>
        /// 此资源组的所有远端资源包
        /// </summary>
        private List<string> remoteBundles = new List<string>();

        /// <summary>
        /// 此资源组的所有远端资源包数量
        /// </summary>
        public int RemoteCount => remoteBundles.Count;

        /// <summary>
        /// 此资源组的所有远端资源包长度
        /// </summary>
        public ulong RemoteLength { get; internal set; }

        /// <summary>
        /// 添加本地资源包
        /// </summary>
        internal void AddLocalBundle(string bundleRelativePath)
        {
            localBundles.Add(bundleRelativePath);
        }

        /// <summary>
        /// 添加远端资源包
        /// </summary>
        internal void AddRemoteBundle(string bundleRelativePath)
        {
            remoteBundles.Add(bundleRelativePath);
        }


    }
}
