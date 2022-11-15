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
        
        public bool IsCompleted => handler.IsDone;

        public T GetResult()
        {
            return handler;
        }
        
        public void OnCompleted(Action continuation)
        {
            handler.AsyncStateMachineMoveNext = continuation;
        }
    }
}