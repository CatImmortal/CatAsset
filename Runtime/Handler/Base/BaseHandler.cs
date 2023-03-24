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
        public string Name { get; private set; }

        /// <summary>
        /// 持有此句柄的任务
        /// </summary>
        internal BaseTask Task { get; set; }

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress
        {
            get
            {
                switch (State)
                {
                    case HandlerState.InValid:
                        return 0;
                        
                    case HandlerState.Doing:
                        return Task?.MainTask.Progress ?? 0;
                    
                    case HandlerState.Success:
                    case HandlerState.Failed:
                        return 1;
                    
                    default:
                        return 0;
                }
            }
        }
        
        /// <summary>
        /// 优先级
        /// </summary>
        public TaskPriority Priority
        {
            get => Task == null ? default : Task.MainTask.Group.Priority;
            set => Task?.Owner.ChangePriority(Task.MainTask,value);
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; internal set; }

        /// <summary>
        /// async/await异步状态机的MoveNext，在加载结束时调用
        /// </summary>
        internal Action AsyncStateMachineMoveNext;
        
        /// <summary>
        /// 被取消时的回调
        /// </summary>
        protected CanceledCallback OnCanceledCallback;
        
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

                OnCanceledCallback += value;
            }

            remove
            {
                if (!IsValid)
                {
                    Debug.LogError($"在无效的{GetType().Name}：{Name}上移除了OnCanceled回调");
                    return;
                }

                OnCanceledCallback -= value;
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
        /// 是否执行中
        /// </summary>
        public bool IsDoing => State == HandlerState.Doing;
        
        /// <summary>
        /// 是否执行成功
        /// </summary>
        public bool IsSuccess => State == HandlerState.Success;
        
        /// <summary>
        /// 是否执行完毕
        /// </summary>
        public bool IsDone => State == HandlerState.Success || State == HandlerState.Failed;
        
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
                    //准备中或加载中 取消
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
        /// 取消执行
        /// </summary>
        protected virtual void Cancel()
        {
            Task?.Cancel();
        }
        
        /// <summary>
        /// 通知句柄已被取消
        /// </summary>
        internal void NotifyCanceled(CancellationToken token)
        {
            var callback = OnCanceledCallback;
            Debug.LogWarning($"{GetType().Name}：{Name}被取消了");
            Release();
            callback?.Invoke(token);
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

        protected void CreateBase(string name)
        {
            Name = name;
            State = HandlerState.Doing;
        }
        
        public virtual void Clear()
        {
            //Name = default; Name不清空了 报错需要
            Task = default;
            Error = default;
            OnCanceledCallback = default;
            AsyncStateMachineMoveNext = default;
            State = default;
        }
    }
}