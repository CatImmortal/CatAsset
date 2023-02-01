using System;
using UnityEngine.UIElements;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器更新资源包信息
    /// </summary>
    public class ProfilerUpdateBundleInfo : IReference,IComparable<ProfilerUpdateBundleInfo>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;
        
        /// <summary>
        /// 状态
        /// </summary>
        public UpdateState State;

        /// <summary>
        /// 总长度
        /// </summary>
        public ulong Length;
        
        /// <summary>
        /// 已更新长度
        /// </summary>
        public ulong UpdatedLength;

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress => (UpdatedLength * 1.0f) / Length;

        public static ProfilerUpdateBundleInfo Create(string name, UpdateState state, ulong length, ulong updatedLength)
        {
            ProfilerUpdateBundleInfo info = ReferencePool.Get<ProfilerUpdateBundleInfo>();
            info.Name = name;
            info.State = state;
            info.Length = length;
            info.UpdatedLength = updatedLength;
            return info;
        }
        
        public void Clear()
        {
            Name = default;
            State = default;
            Length = default;
            UpdatedLength = default;
        }

        public int CompareTo(ProfilerUpdateBundleInfo other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}