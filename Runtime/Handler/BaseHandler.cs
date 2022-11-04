using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 句柄基类
    /// </summary>
    public abstract class BaseHandler : IReference
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
            IsValid = default;
        }
    }
}