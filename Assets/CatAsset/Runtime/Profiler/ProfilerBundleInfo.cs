using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器资源包信息
    /// </summary>
    [Serializable]
    public class ProfilerBundleInfo : IReference, IComparable<ProfilerBundleInfo>,IDependencyChainOwner<ProfilerBundleInfo>
    {
        /// <summary>
        /// 相对路径
        /// </summary>
        public string RelativePath;

        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

        /// <summary>
        /// 是否为原生资源包
        /// </summary>
        public bool IsRaw;

        /// <summary>
        /// 文件长度
        /// </summary>
        public ulong Length;

        /// <summary>
        /// 当前被引用中的资源数量
        /// </summary>
        public int ReferencingAssetCount;

        /// <summary>
        /// 总资源数量
        /// </summary>
        public int TotalAssetCount;

        /// <summary>
        /// 在内存中的资源列表（索引）
        /// </summary>
        public List<int> InMemoryAssetIndexes = new List<int>();

        /// <summary>
        ///  在内存中的资源列表
        /// </summary>
        [NonSerialized]
        public List<ProfilerAssetInfo> InMemoryAssets = new List<ProfilerAssetInfo>();

        /// <summary>
        /// 上游资源包索引
        /// </summary>
        public List<int> UpStreamIndexes = new List<int>();

        /// <summary>
        /// 下游资源包索引
        /// </summary>
        public List<int> DownStreamIndexes = new List<int>();

        /// <summary>
        /// 资源包依赖链
        /// </summary>
        public DependencyChain<ProfilerBundleInfo> DependencyChain { get; } = new DependencyChain<ProfilerBundleInfo>();

        public override string ToString()
        {
            return RelativePath;
        }

        public int CompareTo(ProfilerBundleInfo other)
        {
            return RelativePath.CompareTo(other.RelativePath);
        }

        public static ProfilerBundleInfo Create(string relativePath,string group,bool isRaw,ulong length,int referencingAssetCount,int totalAssetCount)
        {
            ProfilerBundleInfo info = ReferencePool.Get<ProfilerBundleInfo>();
            info.RelativePath = relativePath;
            info.Group = group;
            info.IsRaw = isRaw;
            info.Length = length;
            info.ReferencingAssetCount = referencingAssetCount;
            info.TotalAssetCount = totalAssetCount;
            return info;
        }

        public void Clear()
        {
            RelativePath = default;
            Group = default;
            IsRaw = default;
            Length = default;
            ReferencingAssetCount = default;
            TotalAssetCount = default;

            InMemoryAssetIndexes.Clear();
            InMemoryAssets.Clear();

            UpStreamIndexes.Clear();
            DownStreamIndexes.Clear();
            DependencyChain.UpStream.Clear();
            DependencyChain.DownStream.Clear();
        }



    }
}
