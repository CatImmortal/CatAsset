using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 任务基类
    /// </summary>
    public abstract class BaseTask
    {
        protected BaseTask(TaskExcutor owner, string name)
        {
            this.owner = owner;
            Name = name;
        }

        /// <summary>
        /// 持有此任务的执行器
        /// </summary>

        protected TaskExcutor owner;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus TaskState;

        /// <summary>
        /// 任务完成回调
        /// </summary>
        internal virtual Delegate FinishedCallback
        {
            get;
            set;
        }

        /// <summary>
        /// 任务进度
        /// </summary>
        public virtual float Progress
        {
            get;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// 轮询任务
        /// </summary>
        public abstract void Update();

        public override string ToString()
        {
            return Name;
        }
    }

}
