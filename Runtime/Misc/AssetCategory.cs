namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源类别
    /// </summary>
    public enum AssetCategory
    {
        /// <summary>
        /// 内置资源包资源
        /// </summary>
        InternalBundleAsset,
        
        /// <summary>
        /// 内置原生资源
        /// </summary>
        InternalRawAsset,
        
        /// <summary>
        /// 外置原生资源
        /// </summary>
        ExternalRawAsset,
    }
}