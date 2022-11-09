using System;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 可绑定句柄的接口
    /// </summary>
    public interface IBindableHandler : IDisposable
    {
        /// <summary>
        /// 句柄状态
        /// </summary>
        HandlerState State { get; }
    }
}