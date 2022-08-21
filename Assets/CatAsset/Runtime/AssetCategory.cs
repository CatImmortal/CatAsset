namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源类别
    /// </summary>
    public enum AssetCategory
    {
        /// <summary>
        /// 内置Unity资源
        /// </summary>
        InternalUnityAsset,
        
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