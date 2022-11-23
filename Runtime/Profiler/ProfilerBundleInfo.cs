using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器资源包信息
    /// </summary>
    [Serializable]
    public class ProfilerBundleInfo
    {
        /// <summary>
        /// 目录名
        /// </summary>
        public string Directory;

        /// <summary>
        /// 资源包名
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 相对路径
        /// </summary>
        public string RelativePath;

        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

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

        public List<int> UpStreamIndexes = new List<int>();
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
    }
}
