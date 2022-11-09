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
        /// Awaiter的Continuation回调，在加载完毕时调用
        /// </summary>
        internal Action ContinuationCallBack;

        /// <summary>
        /// 取消用Token
        /// </summary>
        protected CancellationToken Token { get; private set; }


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
        /// 取消加载，同时会释放此句柄
        /// </summary>
        public virtual void Cancel()
        {
            if (State == HandlerState.InValid)
            {
                Debug.LogWarning($"取消了无效的{GetType().Name}：{Name}");
                return;
            }
            
            Task?.Cancel();
            Release();
        }

        /// <summary>
        /// 卸载资源，同时会释放此句柄
        /// </summary>
        public abstract void Unload();

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
        
        /// <summary>
        /// 根据句柄状态进行释放处理
        /// </summary>
        public void Dispose()
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
                    //加载成功 卸载
                    Unload();
                    return;
                
                case HandlerState.Failed:
                    //加载失败 仅释放句柄
                    Release();
                    return;
            }
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
            ContinuationCallBack = default;
            Token = default;
        }
    }
}