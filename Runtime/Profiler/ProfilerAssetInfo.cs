using System;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器资源信息
    /// </summary>
    [Serializable]
    public class ProfilerAssetInfo
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
        /// 资源依赖链
        /// </summary>
        public DependencyChain<ProfilerAssetInfo> DependencyChain { get; } = new DependencyChain<ProfilerAssetInfo>();
    }
}
