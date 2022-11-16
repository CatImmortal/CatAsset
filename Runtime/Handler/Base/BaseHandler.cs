using System;
using System.Threading;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 取消回调方法的原型
    /// </summary>
    public delegate void CanceledCallback(CancellationToken token);
    
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
        /// 被取消时的回调
        /// </summary>
        private CanceledCallback onCanceledCallback;
        
        /// <summary>
        /// 被取消时的回调
        /// </summary>
        public event CanceledCallback OnCanceled
        {
            add
            {
                if (!IsValid)
                {
                    Debug.LogError($"在无效的{GetType().Name}：{Name}上添加了OnCanceled回调");
                    return;
                }

                if (IsDone)
                {
                    return;
                }

                onCanceledCallback += value;
            }

            remove
            {
                if (!IsValid)
                {
                    Debug.LogError($"在无效的{GetType().Name}：{Name}上移除了OnCanceled回调");
                    return;
                }

                onCanceledCallback -= value;
            }
        }

        /// <summary>
        /// async/await异步状态机的MoveNext，在加载结束时调用
        /// </summary>
        internal Action AsyncStateMachineMoveNext;

        /// <summary>
        /// 取消Token
        /// </summary>
        protected CancellationToken Token { get; private set; }

        /// <summary>
        /// 是否已被Token取消
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
        /// 是否有效
        /// </summary>
        public bool IsValid => State != HandlerState.InValid;
        
        /// <summary>
        /// 是否加载中
        /// </summary>
        public bool IsDoing => State == HandlerState.Doing;
        
        /// <summary>
        /// 是否加载成功
        /// </summary>
        public bool IsSuccess => State == HandlerState.Success;
        
        /// <summary>
        /// 是否加载完毕
        /// </summary>
        public bool IsDone => State == HandlerState.Success || State == HandlerState.Failed;

        /// <summary>
        /// 检查是否已被Token取消
        /// </summary>
        protected bool CheckTokenCanceled()
        {
            if (IsTokenCanceled)
            {
                var callback = onCanceledCallback;
                Debug.LogWarning($"{GetType().Name}：{Name}被取消了");
                Unload();
                callback?.Invoke(Token);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查加载失败的错误信息
        /// </summary>
        protected void CheckError()
        {
            if (!IsSuccess && !string.IsNullOrEmpty(Error))
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
            var callback = onCanceledCallback;
            Debug.LogWarning($"{GetType().Name}：{Name}被取消了");
            Task?.Cancel();
            Release();
            
            callback?.Invoke(Token);
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
            if (!IsValid)
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
            //Name = default; Name不清空了 报错需要
            Task = default;
            Error = default;
            onCanceledCallback = default;
            AsyncStateMachineMoveNext = default;
            Token = default;
            State = default;
        }
    }
}