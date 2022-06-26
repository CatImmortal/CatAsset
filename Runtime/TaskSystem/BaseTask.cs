using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务基类
    /// </summary>
    public abstract class BaseTask<T> : ITask where T : ITask
    {

        /// <inheritdoc />
        public TaskRunner Owner { get; private set; }
        
        /// <inheritdoc />
        public string Name { get; private set; }

        /// <inheritdoc />
        public TaskState State { get; set; }
        
        /// <inheritdoc />
        public virtual float Progress { get; }

        /// <summary>
        /// 已合并的任务列表（同名的任务）
        /// </summary>
        protected List<T> mergedTasks = new List<T>();
        
        /// <inheritdoc />
        public int MergedTaskCount => mergedTasks.Count;
        
        /// <inheritdoc />
        public void MergeTask(ITask task)
        {
            mergedTasks.Add((T)task);
        }
        
        /// <inheritdoc />
        public abstract void Run();
        
        /// <inheritdoc />
        public abstract void Update();

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
            Name = name;
        }
        
        /// <inheritdoc />
        public virtual void Clear()
        {
            foreach (T task in mergedTasks)
            {
                ReferencePool.Release(task);
            }
            mergedTasks.Clear();
            
            Owner = default;
            Name = default;
            State = default;
        }



    }

}
