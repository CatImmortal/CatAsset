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
        [Serializable]
        public class BundleInfo: IReference,IComparable<BundleInfo>
        {
            public string Name;
            public BundleRuntimeInfo.State State;
            public ulong Length;

            public static BundleInfo Create(string name,BundleRuntimeInfo.State state,ulong length)
            {
                BundleInfo info = new BundleInfo();
                info.Name = name;
                info.State = state;
                info.Length = length;
                return info;
            }
            
            public void Clear()
            {
                Name = default;
                State = default;
                Length = default;
            }

            public int CompareTo(BundleInfo other)
            {
                return Name.CompareTo(Name);
            }
        }
        
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 此资源组的所有本地资源包
        /// </summary>
        public List<BundleInfo> LocalBundles = new List<BundleInfo>();

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
        public List<BundleInfo> RemoteBundles = new List<BundleInfo>();

        /// <summary>
        /// 远端资源数
        /// </summary>
        public int RemoteCount => RemoteBundles.Count;

        /// <summary>
        /// 远端资源长度
        /// </summary>
        public ulong RemoteLength;

        
        public static ProfilerGroupInfo Create(string name,List<BundleInfo> localBundles,ulong localLength,List<BundleInfo> remoteBundles,ulong remoteLength)
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
            foreach (BundleInfo info in LocalBundles)
            {
                ReferencePool.Release(info);
            }
            LocalBundles.Clear();
            LocalLength = default;
            foreach (BundleInfo info in RemoteBundles)
            {
                ReferencePool.Release(info);
            }
            RemoteBundles.Clear();
            RemoteLength = default;
        }

        public int CompareTo(ProfilerGroupInfo other)
        {
            return Name.CompareTo(Name);
        }
    }
}
