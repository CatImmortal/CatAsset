using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器资源信息
    /// </summary>
    [Serializable]
    public class ProfilerAssetInfo : IReference
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string Name;

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length;

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
        [NonSerialized]
        public DependencyChain<ProfilerAssetInfo> DependencyChain = new DependencyChain<ProfilerAssetInfo>();

        public override string ToString()
        {
            return Name;
        }

        public static ProfilerAssetInfo Create(string name,long length,int refCount)
        {
            ProfilerAssetInfo info = ReferencePool.Get<ProfilerAssetInfo>();
            info.Name = name;
            info.Length = length;
            info.RefCount = refCount;
            return info;
        }

        public void Clear()
        {
            Name = default;
            Length = default;
            RefCount = default;

            UpStreamIndexes.Clear();
            DownStreamIndexes.Clear();
        }
    }
}
