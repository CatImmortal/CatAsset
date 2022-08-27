namespace CatAsset.Runtime
{
    /// <summary>
    /// 版本检查状态
    /// </summary>
    public enum CheckState
    {
        /// <summary>
        /// 需要更新
        /// </summary>
        NeedUpdate,

        /// <summary>
        /// 最新版本存在于读写区
        /// </summary>
        InReadWrite,

        /// <summary>
        /// 最新版本存在于只读区
        /// </summary>
        InReadOnly,

        /// <summary>
        /// 已废弃
        /// </summary>
        Disuse,
    }
}