
namespace CatAsset
{
    /// <summary>
    /// 资源更新检查状态
    /// </summary>
    public enum CheckStatus
    {
        None,

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