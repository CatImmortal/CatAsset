using System;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 可绑定句柄的接口
    /// </summary>
    public interface IBindableHandler
    {
        /// <summary>
        /// 句柄状态
        /// </summary>
        HandlerState State { get; }

        /// <summary>
        /// 卸载句柄，会根据句柄状态进行不同的处理
        /// </summary>
        void Unload();
    }
}