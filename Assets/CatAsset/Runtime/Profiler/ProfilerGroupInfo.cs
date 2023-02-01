using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器资源组信息
    /// </summary>
    [Serializable]
    public class ProfilerGroupInfo : IReference,IComparable<ProfilerGroupInfo>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 此资源组的所有本地资源包
        /// </summary>
        public List<string> LocalBundles = new List<string>();

        /// <summary>
        /// 本地资源数
        /// </summary>
        public int LocalCount => LocalBundles.Count;

        /// <summary>
        /// 本地资源长度
        /// </summary>
        public ulong LocalLength;

        /// <summary>
        /// 此资源组的所有远端资源包
        /// </summary>
        public List<string> RemoteBundles = new List<string>();

        /// <summary>
        /// 远端资源数
        /// </summary>
        public int RemoteCount => RemoteBundles.Count;

        /// <summary>
        /// 远端资源长度
        /// </summary>
        public ulong RemoteLength;



        public static ProfilerGroupInfo Create(string name,List<string> localBundles,ulong localLength,List<string> remoteBundles,ulong remoteLength)
        {
            ProfilerGroupInfo info = ReferencePool.Get<ProfilerGroupInfo>();
            info.Name = name;
            info.LocalBundles.AddRange(localBundles);
            info.LocalLength = localLength;
            info.RemoteBundles.AddRange(remoteBundles);
            info.RemoteLength = remoteLength;
            return info;
        }

        public void Clear()
        {
            Name = default;
            LocalBundles.Clear();
            LocalLength = default;
            RemoteBundles.Clear();
            RemoteLength = default;
        }

        public int CompareTo(ProfilerGroupInfo other)
        {
            return Name.CompareTo(Name);
        }
    }
}
