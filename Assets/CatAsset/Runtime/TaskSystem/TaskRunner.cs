using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务运行器
    /// </summary>
    public class TaskRunner
    {
        public TaskRunner(TaskPriority priority)
        {
            Priority = priority;
        }
        
        /// <summary>
        /// 运行中的任务列表
        /// </summary>
        private List<ITask> runningTasks = new List<ITask>();

        /// <summary>
        /// 任务名->父任务
        /// </summary>
        private Dictionary<string, ITask> parentTaskDict = new Dictionary<string, ITask>();

        /// <summary>
        /// 需要添加的任务
        /// </summary>
        private List<ITask> waitAddTasks = new List<ITask>();

        /// <summary>
        /// 需要移除的任务序号
        /// </summary>
        private List<int> waitRemoveTasks = new List<int>();
        
        /// <summary>
        /// 任务优先级
        /// </summary>
        public readonly TaskPriority Priority;

        /// <summary>
        /// 单帧最大任务运行次数
        /// </summary>
        public int MaxRunCount { get; set; } = int.MaxValue;

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(ITask task)
        {
            waitAddTasks.Add(task);
        }

        public void Update()
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

            //运行任务
            if (runningTasks.Count > 0)
            {
                int runCount = 0;
                for (int i = 0; i < runningTasks.Count; i++)
                {
                    if (i >= MaxRunCount)
                    {
                        //当前帧运行的任务数量已达上限
                        break;
                    }

                    ITask task = runningTasks[i];
                    if (task.State == TaskState.Free)
                    {
                        //运行空闲状态的任务
                        task.Run();
                    }

                    //轮询任务
                    task.Update();

                    switch (task.State)
                    {
                        case TaskState.Executing:
                            runCount++;
                            break;
                        case TaskState.Finished:
                            runCount++;
                            
                            //任务运行结束 需要删除
                            waitRemoveTasks.Add(i);
                            break;
                    }
                }
            }
            
            //移除需要移除的任务
            if (waitRemoveTasks.Count > 0)
            {
                for (int i = waitRemoveTasks.Count - 1; i >= 0; i--)
                {
                    int removeIndex = waitRemoveTasks[i];
                    ITask task = runningTasks[removeIndex];
                    
                    runningTasks.RemoveAt(removeIndex);
                    parentTaskDict.Remove(task.Name);
                }

                waitRemoveTasks.Clear();
            }
        }

        /// <summary>
        /// 添加任务（会合并重复任务）
        /// </summary>
        private void InternalAddTask(ITask task)
        {
            if (!parentTaskDict.TryGetValue(task.Name,out ITask parent))
            {
                parent.AddChild(task);
            }
            else
            {
                parentTaskDict.Add(task.Name,task);
                runningTasks.Add(task);
            }
        }
    }
}

