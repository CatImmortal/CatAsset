using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace CatAsset
{
    /// <summary>
    /// 任务执行器
    /// </summary>
    public class TaskExcutor
    {
        /// <summary>
        /// 任务名称与对应Task
        /// </summary>
        private Dictionary<string, BaseTask> taskDict = new Dictionary<string, BaseTask>();

        /// <summary>
        /// 需要添加的任务
        /// </summary>
        private List<BaseTask> needAddTasks = new List<BaseTask>();

        /// <summary>
        /// 需要移除的任务
        /// </summary>
        private List<string> needRemoveTasks = new List<string>();

        /// <summary>
        /// 每帧最多执行任务次数
        /// </summary>
        public int MaxExcuteCount = 10;

        /// <summary>
        /// 是否存在指定任务
        /// </summary>
        public bool HasTask(string taskName)
        {
            return taskDict.ContainsKey(taskName);
        }

        /// <summary>
        /// 是否存在指定任务
        /// </summary>
        public bool HasTask(BaseTask task)
        {
            return taskDict.ContainsKey(task.Name) && task.GetType() == taskDict[task.Name].GetType() ;
        }

        /// <summary>
        /// 获取指定任务的状态
        /// </summary>
        public TaskState GetTaskState(string name)
        {
            return taskDict[name].State;
        }

        /// <summary>
        /// 追加任务执行回调
        /// </summary>
        public void AppendTaskCompleted(string name,Action<object> completed)
        {
            if (!taskDict.TryGetValue(name,out BaseTask task))
            {
                Debug.LogError("AppendTaskCompleted调用失败，此Task不存在：" + name);
                return;
            }

            task.Completed += completed;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(BaseTask task)
        {
            needAddTasks.Add(task);
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        private void InternalAddTask(BaseTask task)
        {
            if (HasTask(task))
            {

                //任务已存在 不需要重复添加
                AppendTaskCompleted(task.Name, task.Completed);
                return;
            }

            taskDict.Add(task.Name, task);
        }

        /// <summary>
        /// 轮询任务
        /// </summary>
        public void Update()
        {
           

            if (needAddTasks.Count > 0)
            {
                //添加需要添加的任务
                for (int i = 0; i < needAddTasks.Count; i++)
                {
                    BaseTask task = needAddTasks[i];
                    InternalAddTask(task);
                }

                needAddTasks.Clear();
            }

            if (taskDict.Count > 0)
            {
                //处理任务

                int executeCount = 0;
                foreach (KeyValuePair<string, BaseTask> item in taskDict)
                {
                    if (executeCount >= MaxExcuteCount)
                    {
                        break;
                    }

                    BaseTask task = item.Value;

                    switch (task.State)
                    {
                        case TaskState.Free:
                            task.Execute();
                            task.UpdateState();
                            executeCount++;
                            break;

                        case TaskState.Waiting:
                            task.UpdateState();
                            break;

                        case TaskState.Executing:
                            task.UpdateState();
                            executeCount++;
                            break;
                    }
                   
                    if (task.State == TaskState.Finished)
                    {
                        //在task.UpdateState执行过后，State可能会变成Finished 这样在当前帧UpdateState后完成的任务就在当前帧移除了
                        needRemoveTasks.Add(task.Name);
                    }

                }
            }

            //移除需要移除的任务
            if (needRemoveTasks.Count > 0)
            {
                for (int i = 0; i < needRemoveTasks.Count; i++)
                {
                    string taskName = needRemoveTasks[i];
                    taskDict.Remove(taskName);
                }

                needRemoveTasks.Clear();
            }
        }
    }
}


