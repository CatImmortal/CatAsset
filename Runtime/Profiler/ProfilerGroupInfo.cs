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
        public List<string> LocalBundles;
        
        /// <summary>
        /// 本地资源数
        /// </summary>
        public int LocalCount;

        /// <summary>
        /// 本地资源长度
        /// </summary>
        public ulong LocalLength;

        /// <summary>
        /// 此资源组的所有远端资源包
        /// </summary>
        public List<string> RemoteBundles;
        
        /// <summary>
        /// 远端资源数
        /// </summary>
        public int RemoteCount;

        /// <summary>
        /// 远端资源长度
        /// </summary>
        public ulong RemoteLength;



        public static ProfilerGroupInfo Create(string name,List<string> localBundles,int localCount,ulong localLength,List<string> remoteBundles,int remoteCount,ulong remoteLength)
        {
            ProfilerGroupInfo info = ReferencePool.Get<ProfilerGroupInfo>();
            info.Name = name;
            info.LocalBundles = localBundles;
            info.LocalCount = localCount;
            info.LocalLength = localLength;
            info.RemoteBundles = remoteBundles;
            info.RemoteCount = remoteCount;
            info.RemoteLength = remoteLength;
            return info;
        }

        public void Clear()
        {
            Name = default;
            LocalBundles = default;
            LocalCount = default;
            LocalLength = default;
            RemoteBundles = default;
            RemoteCount = default;
            RemoteLength = default;
        }

        public int CompareTo(ProfilerGroupInfo other)
        {
            return Name.CompareTo(Name);
        }
    }
}
