using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务基类
    /// </summary>
    public abstract class BaseTask<T> : ITask where T : ITask
    {
        /// <summary>
        /// 已合并的任务列表（同名的任务）
        /// </summary>
        protected readonly List<T> mergedTasks = new List<T>();

        protected BaseTask(TaskRunner owner, string name)
        {
            Owner = owner;
            Name = name;
        }
        
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public TaskState State { get; protected set; }
        
        /// <inheritdoc />
        public TaskRunner Owner { get; }
        
        /// <inheritdoc />
        public virtual float Progress { get; }

        /// <inheritdoc />
        public void AddChild(ITask child)
        {
            mergedTasks.Add((T)child);
        }
        
        /// <inheritdoc />
        public abstract void Run();
        
        /// <inheritdoc />
        public abstract void Update();

        public override string ToString()
        {
            return Name;
        }
    }

}
