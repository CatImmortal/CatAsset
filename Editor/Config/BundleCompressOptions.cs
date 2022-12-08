namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包压缩设置
    /// </summary>
    public enum BundleCompressOptions
    {
        /// <summary>
        /// 使用全局设置
        /// </summary>
        UseGlobal,
        
        /// <summary>
        /// 不压缩
        /// </summary>
        Uncompressed,
        
        /// <summary>
        /// LZ4压缩
        /// </summary>
        LZ4,
        
        /// <summary>
        /// LZMA压缩
        /// </summary>
        LZMA,
    }
}