using System.Collections.Generic;
using UnityEngine;

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
        public int GUID { get; protected set; }
        
        /// <inheritdoc />
        public string Name { get; private set; }

        /// <inheritdoc />
        public TaskState State { get; set; }
        
        /// <inheritdoc />
        public virtual float Progress { get; }

        /// <summary>
        /// 已合并的任务列表（同名的任务）
        /// </summary>
        protected List<T> MergedTasks = new List<T>();
        
        /// <inheritdoc />
        public int MergedTaskCount => MergedTasks.Count;
        
        /// <inheritdoc />
        public void MergeTask(ITask task)
        {
            MergedTasks.Add((T)task);
        }
        
        /// <inheritdoc />
        public abstract void Run();
        
        /// <inheritdoc />
        public abstract void Update();

        /// <inheritdoc />
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
            GUID = ++TaskRunner.GUIDFactory;
            CatAssetManager.AddTaskGUID(this);
            Name = name;
        }
        
        /// <inheritdoc />
        public virtual void Clear()
        {
            foreach (T task in MergedTasks)
            {
                ReferencePool.Release(task);
            }
            MergedTasks.Clear();
            
            Owner = default;
            CatAssetManager.RemoveTaskGUID(this);
            GUID = default;
            Name = default;
            State = default;
        }



    }

}
