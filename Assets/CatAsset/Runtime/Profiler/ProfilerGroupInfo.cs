using System;

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
        /// 远端资源数
        /// </summary>
        public int RemoteCount;

        /// <summary>
        /// 远端资源长度
        /// </summary>
        public long RemoteLength;

        /// <summary>
        /// 本地资源数
        /// </summary>
        public int LocalCount;

        /// <summary>
        /// 本地资源长度
        /// </summary>
        public long LocalLength;

        public static ProfilerGroupInfo Create(string name,int remoteCount,long remoteLength,int localCount,long localLength)
        {
            ProfilerGroupInfo info = ReferencePool.Get<ProfilerGroupInfo>();
            info.Name = name;
            info.RemoteCount = remoteCount;
            info.RemoteLength = remoteLength;
            info.LocalCount = localCount;
            info.LocalLength = localLength;
            return info;
        }

        public void Clear()
        {
            Name = default;
            RemoteCount = default;
            RemoteLength = default;
            LocalCount = default;
            LocalLength = default;
        }

        public int CompareTo(ProfilerGroupInfo other)
        {
            return Name.CompareTo(Name);
        }
    }
}
