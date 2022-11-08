using System;
using System.Runtime.CompilerServices;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 句柄的可等待对象
    /// </summary>
    public readonly struct HandlerAwaiter<T> : INotifyCompletion where T : BaseHandler
    {
        private readonly T handler;

        public HandlerAwaiter(T handler)
        {
            this.handler = handler;
        }
        
        //如果加载成功 那么Handler的状态是Success
        //如果加载失败 那么Handler的状态可能是Failed 或者 Invalid
        public bool IsCompleted => handler.State != HandlerState.Doing;

        public T GetResult()
        {
            return handler;
        }
        
        public void OnCompleted(Action continuation)
        {
            handler.ContinuationCallBack = continuation;
        }
    }
}