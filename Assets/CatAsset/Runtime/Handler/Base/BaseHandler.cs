using System;
using System.Threading;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 句柄基类
    /// </summary>
    public abstract class BaseHandler : IReference, IDisposable
    {
        /// <summary>
        /// 句柄名
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 持有此句柄的任务
        /// </summary>
        internal BaseTask Task { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; internal set; }
        
        /// <summary>
        /// Awaiter的Continuation回调，在加载完毕时调用
        /// </summary>
        internal Action ContinuationCallBack;

        /// <summary>
        /// 取消Token
        /// </summary>
        protected CancellationToken Token { get; private set; }

        /// <summary>
        /// 是否被Token取消
        /// </summary>
        internal bool IsTokenCanceled => Token != default && Token.IsCancellationRequested;
        
        /// <summary>
        /// 进度
        /// </summary>
        public float Progress
        {
            get
            {
                switch (State)
                {
                    case HandlerState.Doing:
                        return Task?.Progress ?? 0;
                    
                    case HandlerState.Success:
                    case HandlerState.Failed:
                        return 1;
                    
                    default:
                        return 0;
                }
            }
        }
        
        /// <summary>
        /// 句柄状态
        /// </summary>
        public HandlerState State { get; protected set; }

        /// <summary>
        /// 检查加载失败的错误信息
        /// </summary>
        protected void CheckError()
        {
            if (State == HandlerState.Failed && !string.IsNullOrEmpty(Error))
            {
                Debug.LogError($"{GetType().Name} | {Name} 加载失败：{Error}");
            }
        }
        
        /// <summary>
        /// 卸载句柄，会根据句柄状态进行不同的处理
        /// </summary>
        public void Unload()
        {
            switch (State)
            {
                case HandlerState.InValid:
                    //无效句柄 不处理
                    return;
                
                case HandlerState.Doing:
                    //加载中 取消
                    Cancel();
                    return;
                
                case HandlerState.Success:
                    //加载成功 卸载资源
                    InternalUnload();
                    return;
                
                case HandlerState.Failed:
                    //加载失败 仅释放句柄
                    Release();
                    return;
            }
        }

        /// <summary>
        /// 取消加载，同时会释放此句柄
        /// </summary>
        protected virtual void Cancel()
        {
            Task?.Cancel();
            Release();
        }
        
        /// <summary>
        /// 卸载资源，同时会释放此句柄
        /// </summary>
        protected abstract void InternalUnload();

        /// <summary>
        /// 释放句柄，会将此句柄归还引用池
        /// </summary>
        internal void Release()
        {
            if (State == HandlerState.InValid)
            {
                Debug.LogError($"错误的释放了无效的{GetType().Name}：{Name}");
                return;
            }
            ReferencePool.Release(this);
        }
        
        public void Dispose()
        {
            Unload();
        }

        public void CreateBase(string name,CancellationToken token)
        {
            Name = name;
            Token = token;
            State = HandlerState.Doing;
        }
        
        public virtual void Clear()
        {
            //Name = default; Name就不清空了 方便Debug
            Task = default;
            State = default;
            Error = default;
            ContinuationCallBack = default;
            Token = default;
        }
    }
}