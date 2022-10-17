using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包依赖链（如A依赖B，B依赖C，则称A为B的上游，C为B的下游）
    /// </summary>
    public class BundleDependencyLink
    {
        /// <summary>
        /// 上游资源包集合(依赖此资源包的资源包)
        /// </summary>
        public readonly HashSet<BundleRuntimeInfo> UpStream = new HashSet<BundleRuntimeInfo>();
        
        /// <summary>
        /// 下游资源包集合（此资源包依赖的资源包）
        /// </summary>
        public readonly HashSet<BundleRuntimeInfo> DownStream = new HashSet<BundleRuntimeInfo>();
    }
}