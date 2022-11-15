namespace CatAsset.Runtime
{
    /// <summary>
    /// 依赖链持有者接口
    /// </summary>
    public interface IDependencyChainOwner<T> where T : IDependencyChainOwner<T>
    {
        /// <summary>
        /// 依赖链
        /// </summary>
        public DependencyChain<T> DependencyChain { get; }
    }
}