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
        /// 本地资源数
        /// </summary>
        public int LocalCount;

        /// <summary>
        /// 本地资源长度
        /// </summary>
        public ulong LocalLength;

        /// <summary>
        /// 远端资源数
        /// </summary>
        public int RemoteCount;

        /// <summary>
        /// 远端资源长度
        /// </summary>
        public ulong RemoteLength;



        public static ProfilerGroupInfo Create(string name,int localCount,ulong localLength,int remoteCount,ulong remoteLength)
        {
            ProfilerGroupInfo info = ReferencePool.Get<ProfilerGroupInfo>();
            info.Name = name;
            info.LocalCount = localCount;
            info.LocalLength = localLength;
            info.RemoteCount = remoteCount;
            info.RemoteLength = remoteLength;
            return info;
        }

        public void Clear()
        {
            Name = default;
            LocalCount = default;
            LocalLength = default;
            RemoteCount = default;
            RemoteLength = default;
        }

        public int CompareTo(ProfilerGroupInfo other)
        {
            return Name.CompareTo(Name);
        }
    }
}
