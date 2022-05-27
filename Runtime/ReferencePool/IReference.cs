namespace CatAsset.Runtime
{
    /// <summary>
    /// 引用池对象接口
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// 清理引用
        /// </summary>
        void Clear();
    }
}