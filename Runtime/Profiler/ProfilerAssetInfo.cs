using System;
using System.Collections.Generic;

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

        public List<int> UpStreamIndexes = new List<int>();
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
    }
}
