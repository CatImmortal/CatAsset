using System;
using System.Runtime.CompilerServices;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 句柄的可等待对象
    /// </summary>
    public readonly struct HandlerAwaiter : INotifyCompletion
    {
        private readonly BaseHandler handler;

        public HandlerAwaiter(BaseHandler handler)
        {
            this.handler = handler;
        }
        
        public bool IsCompleted => handler.IsDone;

        public void GetResult()
        {
            
        }
        
        public void OnCompleted(Action continuation)
        {
            handler.AwaiterContinuation = continuation;
        }
    }
}