using System;

using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 句柄基类
    /// </summary>
    public abstract class BaseHandler : IReference, IDisposable
    {
        /// <summary>
        /// 持有此句柄的任务
        /// </summary>
        internal BaseTask Task;

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress => Task?.Progress ?? 0;
        
        /// <summary>
        /// 是否已加载完毕
        /// </summary>
        public bool IsDone { get; protected set; }
        
        /// <summary>
        /// 是否加载成功
        /// </summary>
        public abstract bool Success { get; }

        /// <summary>
        /// Awaiter的Continuation回调，在加载完毕时调用
        /// </summary>
        internal Action AwaiterContinuation;
        
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; protected set; }

        /// <summary>
        /// 取消加载，同时会释放此句柄
        /// </summary>
        public virtual void Cancel()
        {
            Task?.Cancel();
            Release();
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// 释放句柄，会将此句柄归还引用池
        /// </summary>
        public void Release()
        {
            if (!IsValid)
            {
                Debug.LogError($"错误的释放了无效的{GetType().Name}");
                return;
            }
            ReferencePool.Release(this);
        }
        
        public virtual void Clear()
        {
            Task = default;
            IsDone = default;
            AwaiterContinuation = default;
            IsValid = default;
        }
        
        public void Dispose()
        {
            if (!IsValid)
            {
                //无效句柄 不处理
                return;
            }
            
            if (!IsDone)
            {
                //加载未完毕 取消
                Cancel();
                return;
            }
            
            if (Success)
            {
                //加载成功 卸载
                Unload();
            }
            else
            {
                //加载失败 仅释放句柄
                Release();
            }
        }
        
        public HandlerAwaiter GetAwaiter()
        {
            return new HandlerAwaiter(this);
        }

    }
}