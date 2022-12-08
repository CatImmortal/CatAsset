using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器资源信息
    /// </summary>
    [Serializable]
    public class ProfilerAssetInfo : IReference,IDependencyChainOwner<ProfilerAssetInfo>
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string Name;

        /// <summary>
        /// 资源组
        /// </summary>
        [NonSerialized]
        public string Group;

        /// <summary>
        /// 资源包
        /// </summary>
        [NonSerialized]
        public string Bundle;

        /// <summary>
        /// 资源类型
        /// </summary>
        public string Type;

        /// <summary>
        /// 内存大小
        /// </summary>
        public ulong MemorySize;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount;

        /// <summary>
        /// 上游资源索引
        /// </summary>
        public List<int> UpStreamIndexes = new List<int>();

        /// <summary>
        /// 下游资源索引
        /// </summary>
        public List<int> DownStreamIndexes = new List<int>();

        /// <summary>
        /// 资源依赖链
        /// </summary>
        public DependencyChain<ProfilerAssetInfo> DependencyChain { get; } = new DependencyChain<ProfilerAssetInfo>();

        public override string ToString()
        {
            return Name;
        }

        public static ProfilerAssetInfo Create(string name,string type,ulong memorySize,int refCount)
        {
            ProfilerAssetInfo info = ReferencePool.Get<ProfilerAssetInfo>();
            info.Name = name;
            info.Type = type;
            info.MemorySize = memorySize;
            info.RefCount = refCount;
            return info;
        }

        public void Clear()
        {
            Name = default;
            Group = default;
            Bundle = default;
            Type = default;
            MemorySize = default;
            RefCount = default;

            UpStreamIndexes.Clear();
            DownStreamIndexes.Clear();
            DependencyChain.UpStream.Clear();
            DependencyChain.DownStream.Clear();
        }
    }
}
