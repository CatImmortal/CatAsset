using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务基类
    /// </summary>
    public abstract class BaseTask : IReference
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int ID { get; protected set; }
        
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// 持有者
        /// </summary>
        public TaskRunner Owner { get; private set; }

        /// <summary>
        /// 任务组
        /// </summary>
        public TaskGroup Group { get; set; }
        
        /// <summary>
        /// 状态
        /// </summary>
        public TaskState State { get; set; }

        /// <summary>
        /// 子状态
        /// </summary>
        public virtual string SubState { get; } = string.Empty;
        
        /// <summary>
        /// 进度
        /// </summary>
        public virtual float Progress { get; }
        
        /// <summary>
        /// 主任务
        /// </summary>
        public BaseTask MainTask => Owner.GetMainTask(GetType(), Name);
        
        /// <summary>
        /// 已合并的任务列表（同名的任务）
        /// </summary>
        protected readonly List<BaseTask> MergedTasks = new List<BaseTask>();
        
        /// <summary>
        /// 已合并任务数量
        /// </summary>
        public int MergedTaskCount => MergedTasks.Count;

        /// <summary>
        /// 是否被调用过取消方法
        /// </summary>
        private bool isCancelFuncCalled;
        
        /// <summary>
        /// 取消token
        /// </summary>
        protected CancellationToken CancelToken { get; private set; }
        
        /// <summary>
        /// 此任务是否被取消
        /// (被调用过取消方法 或者 被token取消了 就认为任务被取消了)
        /// </summary>
        public bool IsCanceled => isCancelFuncCalled || (CancelToken != default && CancelToken.IsCancellationRequested);

        /// <summary>
        /// 所有同名任务是否被全部取消
        /// </summary>
        public bool IsAllCanceled
        {
            get
            {
                foreach (BaseTask mergedTask in MainTask.MergedTasks)
                {
                    if (!mergedTask.IsCanceled)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        
        /// <summary>
        /// 合并任务
        /// </summary>
        public void MergeTask(BaseTask task)
        {
            MergedTasks.Add(task);
        }
        
        /// <summary>
        /// 运行任务
        /// </summary>
        public abstract void Run();
        
        /// <summary>
        /// 轮询任务
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// 取消任务
        /// </summary>
        public virtual void Cancel()
        {
            isCancelFuncCalled = true;
        }

        /// <summary>
        /// 优先级发生改变时的回调
        /// </summary>
        public virtual void OnPriorityChanged()
        {
        }

        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// 创建基类部分
        /// </summary>
        protected void CreateBase(TaskRunner owner,string name,CancellationToken token = default)
        {
            Owner = owner;
            ID = ++TaskRunner.TaskIDFactory;
            Name = name;
            CancelToken = token;
        }
        
        /// <inheritdoc />
        public virtual void Clear()
        {
            ID = default;
            //Name = default;
            Owner = default;
            Group = default;
            State = default;
            foreach (BaseTask task in MergedTasks)
            {
                if (task == this)
                {
                    continue;
                }
                ReferencePool.Release(task);
            }
            MergedTasks.Clear();
        }



    }

}
