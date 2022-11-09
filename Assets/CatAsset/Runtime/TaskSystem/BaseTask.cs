using System.Collections.Generic;
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
        /// 进度
        /// </summary>
        public virtual float Progress { get; }
        
        /// <summary>
        /// 已合并的任务列表（同名的任务）
        /// </summary>
        protected readonly List<BaseTask> MergedTasks = new List<BaseTask>();
        
        /// <summary>
        /// 已合并任务数量
        /// </summary>
        public int MergedTaskCount => MergedTasks.Count;
        
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
            Debug.LogError($"此任务类型未支持取消操作:{GetType().Name}");
        }

        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// 创建基类部分
        /// </summary>
        protected void CreateBase(TaskRunner owner,string name)
        {
            Owner = owner;
            ID = ++TaskRunner.TaskIDFactory;
            Name = name;
        }
        
        /// <inheritdoc />
        public virtual void Clear()
        {
            ID = default;
            Name = default;
            Owner = default;
            Group = default;
            State = default;
            foreach (BaseTask task in MergedTasks)
            {
                ReferencePool.Release(task);
            }
            MergedTasks.Clear();
        }



    }

}
