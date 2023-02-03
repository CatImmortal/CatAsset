using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器更新器信息
    /// </summary>
    [Serializable]
    public class ProfilerUpdaterInfo : IReference,IComparable<ProfilerUpdaterInfo>
    {

        [Serializable]
        public class BundleInfo : IReference,IComparable<BundleInfo>
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
            /// 长度
            /// </summary>
            public ulong Length;
        
            /// <summary>
            /// 已下载字节数
            /// </summary>
            public ulong DownLoadedBytesLength;

            /// <summary>
            /// 进度
            /// </summary>
            public float Progress => (DownLoadedBytesLength * 1.0f) / Length;

            public static BundleInfo Create(string name, UpdateState state, ulong length, ulong downLoadedBytesLength)
            {
                BundleInfo info = ReferencePool.Get<BundleInfo>();
                info.Name = name;
                info.State = state;
                info.Length = length;
                info.DownLoadedBytesLength = downLoadedBytesLength;
                return info;
            }
        
            public void Clear()
            {
                Name = default;
                State = default;
                Length = default;
                DownLoadedBytesLength = default;
            }

            public int CompareTo(BundleInfo other)
            {
                return Name.CompareTo(other.Name);
            }
        }
        
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 状态
        /// </summary>
        public GroupUpdaterState State;
        
        /// <summary>
        /// 此更新器的所有资源包信息列表
        /// </summary>
        public List<BundleInfo> UpdateBundleInfos;
        
        /// <summary>
        /// 待更新的资源包总数
        /// </summary>
        public int WaitingCount => GetCount(UpdateState.Waiting);
        
        /// <summary>
        /// 待更新的资源包总长度
        /// </summary>
        public ulong WaitingLength => GetLength(UpdateState.Waiting);
        
        /// <summary>
        /// 更新中的资源包总数
        /// </summary>
        public int UpdatingCount => GetCount(UpdateState.Updating);
        
        /// <summary>
        /// 更新中的资源包总长度
        /// </summary>
        public ulong UpdatingLength => GetLength(UpdateState.Updating);

        /// <summary>
        /// 已更新的资源包总数
        /// </summary>
        public int UpdatedCount => GetCount(UpdateState.Updated);
        
        /// <summary>
        /// 已更新的资源包总长度
        /// </summary>
        public ulong UpdatedLength => GetLength(UpdateState.Updated);
        
        
        /// <summary>
        /// 此更新器的所有资源包总数
        /// </summary>
        public int TotalCount => UpdateBundleInfos.Count;

        /// <summary>
        /// 此更新器的所有资源包总长度
        /// </summary>
        public ulong TotalLength;
        
        /// <summary>
        /// 已下载字节数
        /// </summary>
        public ulong DownloadedBytesLength;
        
        /// <summary>
        /// 进度
        /// </summary>
        public float Progress => (DownloadedBytesLength * 1.0f) / TotalLength;
        
        /// <summary>
        /// 下载速度
        /// </summary>
        public ulong Speed;

        /// <summary>
        /// 获取指定状态的更新资源包的数量
        /// </summary>
        private int GetCount(UpdateState state)
        {
            int count = 0;
            foreach (BundleInfo info in UpdateBundleInfos)
            {
                if (info.State == state)
                {
                    count++;
                }
            }

            return count;
        }
        
        /// <summary>
        /// 获取指定状态的更新资源包的长度
        /// </summary>
        private ulong GetLength(UpdateState state)
        {
            ulong length = 0;
            foreach (BundleInfo info in UpdateBundleInfos)
            {
                if (info.State == state)
                {
                    length += info.Length;
                }
            }

            return length;
        }

        /// <summary>
        /// 获取指定状态的更新资源包列表
        /// </summary>
        public List<BundleInfo> GetBundleInfos(UpdateState state)
        {
            List<BundleInfo> result = new List<BundleInfo>();
            foreach (BundleInfo info in UpdateBundleInfos)
            {
                if (info.State == state)
                {
                    result.Add(info);
                }
            }

            return result;
        } 

        public static ProfilerUpdaterInfo Create(string name, GroupUpdaterState state,List<BundleInfo> updateBundleInfos,ulong downloadedBytesLength,ulong speed)
        {
            ProfilerUpdaterInfo info = ReferencePool.Get<ProfilerUpdaterInfo>();
            info.Name = name;
            info.State = state;
            
            info.UpdateBundleInfos = updateBundleInfos;
            foreach (BundleInfo pubi in updateBundleInfos)
            {
                info.TotalLength += pubi.Length;
            }

            info.DownloadedBytesLength = downloadedBytesLength;
            info.Speed = speed;
           
            return info;
        }

        public void Clear()
        {
            Name = default;
            State = default;
            foreach (BundleInfo pubi in UpdateBundleInfos)
            {
                ReferencePool.Release(pubi);
            }
            UpdateBundleInfos = default;
            TotalLength = default;
            DownloadedBytesLength = default;
            Speed = default;
           
        }

        public int CompareTo(ProfilerUpdaterInfo other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}
