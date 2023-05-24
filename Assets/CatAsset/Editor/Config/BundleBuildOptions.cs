using System;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建设置
    /// </summary>
    [Flags]
    public enum BundleBuildOptions
    {
        /// <summary>
        /// 生成LinkXML
        /// </summary>
        WriteLinkXML = 1 << 0,
        
        /// <summary>
        /// 强制全量构建
        /// </summary>
        ForceRebuild = 1 << 1,
        
        /// <summary>
        /// 附加Hash到资源包名中
        /// </summary>
        AppendHash = 1 << 2,
        
        /// <summary>
        /// 关闭TypeTree
        /// </summary>
        DisableTypeTree = 1 << 3,
    }
}