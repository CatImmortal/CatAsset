using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务组
    /// </summary>
    public class TaskGroup
    {
        private List<ITask> runningTasks = new List<ITask>();
        
        private List<ITask> waitAddTasks = new List<ITask>();
        
        private List<int> waitRemoveTasks = new List<int>();

        private Dictionary<string, ITask> mainTaskDict = new Dictionary<string, ITask>();

        /// <summary>
        /// 下一个要运行的任务索引
        /// </summary>
        private int nextRunningTaskIndex;
        
        /// <summary>
        /// 此任务组的优先级
        /// </summary>
        public readonly TaskPriority Priority;

        /// <summary>
        /// 任务组是否能运行
        /// </summary>
        public bool CanRun => nextRunningTaskIndex < runningTasks.Count;

        public TaskGroup(TaskPriority priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(ITask task)
        {
            waitAddTasks.Add(task);
        }
        
        /// <summary>
        /// 添加任务（会合并重复任务）
        /// </summary>
        private void InternalAddTask(ITask task)
        {
            if (mainTaskDict.TryGetValue(task.Name,out ITask mainTask))
            {
                Debug.Log($"合并任务:{task}");
                mainTask.MergeTask(task);
            }
            else
            {
                Debug.Log($"添加任务:{task}");
                mainTaskDict.Add(task.Name,task);
                runningTasks.Add(task);
            }
        }

        /// <summary>
        /// 运行前调用
        /// </summary>
        public void PreRun()
        {
            //添加需要添加的任务
            if (waitAddTasks.Count > 0)
            {
                for (int i = 0; i < waitAddTasks.Count; i++)
                {
                    ITask task = waitAddTasks[i];
                    InternalAddTask(task);
                }
                waitAddTasks.Clear();
            }

          
        }

        /// <summary>
        /// 运行任务组
        /// </summary>
        public bool Run()
        {
            int index = nextRunningTaskIndex;
            nextRunningTaskIndex++;
            
            ITask task = runningTasks[index];
            
            if (task.State == TaskState.Free)
            {
                //运行空闲状态的任务
                Debug.Log($"运行任务:{task}");
                task.Run();
               
            }

            //轮询任务
            Debug.Log($"轮询任务:{task}");
            task.Update();
           
            
            switch (task.State)
            {
                case TaskState.Running:
                    return true;

                case TaskState.Finished:
                    //任务运行结束 需要删除
                    waitRemoveTasks.Add(index);
                    return true;
            }

            return false;

        }
        
        /// <summary>
        /// 运行后调用
        /// </summary>
        public void PostRun()
        {
            nextRunningTaskIndex = 0;
            
            //移除需要移除的任务
            if (waitRemoveTasks.Count > 0)
            {
                for (int i = waitRemoveTasks.Count - 1; i >= 0; i--)
                {
                    int removeIndex = waitRemoveTasks[i];
                    ITask task = runningTasks[removeIndex];
                    
                    Debug.Log($"移除任务:{task}");
                    runningTasks.RemoveAt(removeIndex);
                    mainTaskDict.Remove(task.Name);
                    ReferencePool.Release(task);
                }

                waitRemoveTasks.Clear();
            }
        }
        

        
        
        
    }
}