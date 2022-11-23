using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器资源包信息
    /// </summary>
    [Serializable]
    public class ProfilerBundleInfo : IReference
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
        public long Length;

        /// <summary>
        /// 资源总数
        /// </summary>
        public int AssetCount;

        /// <summary>
        /// 当前被引用中的资源集合，这里面的资源的引用计数都大于0（索引）
        /// </summary>
        public List<int> ReferencingAssetIndexes = new List<int>();

        /// <summary>
        /// 当前被引用中的资源集合，这里面的资源的引用计数都大于0
        /// </summary>
        [NonSerialized]
        public HashSet<ProfilerAssetInfo> ReferencingAssets = new HashSet<ProfilerAssetInfo>();

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
        [NonSerialized]
        public DependencyChain<ProfilerBundleInfo> DependencyChain = new DependencyChain<ProfilerBundleInfo>();

        public override string ToString()
        {
            return RelativePath;
        }

        public static ProfilerBundleInfo Create(string relativePath,string group,bool isRaw,long length,int assetCount)
        {
            ProfilerBundleInfo info = ReferencePool.Get<ProfilerBundleInfo>();
            info.RelativePath = relativePath;
            info.Group = group;
            info.IsRaw = isRaw;
            info.Length = length;
            info.AssetCount = assetCount;
            return info;
        }

        public void Clear()
        {
            RelativePath = default;
            Group = default;
            IsRaw = default;
            Length = default;
            AssetCount = default;

            ReferencingAssetIndexes.Clear();

            UpStreamIndexes.Clear();
            DownStreamIndexes.Clear();
        }
    }
}
