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
        /// 单帧最大任务执行次数
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
            return taskDict.ContainsKey(task.Name);
        }

        /// <summary>
        /// 获取指定任务的状态
        /// </summary>
        public TaskStatus GetTaskState(string name)
        {
            return taskDict[name].TaskState;
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
                if (task.FinishedCallback != null)
                {
                    //任务已存在 不需要重复添加 合并回调即可
                    taskDict[task.Name].FinishedCallback = Delegate.Combine(taskDict[task.Name].FinishedCallback, task.FinishedCallback);
                }
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
                        //当前帧执行的任务数量已达上限

                        break;
                    }

                    BaseTask task = item.Value;

                    if (task.TaskState == TaskStatus.Free)
                    {
                        task.Execute();
                    }

                    task.Update();

                    switch (task.TaskState)
                    {
                        case TaskStatus.Executing:
                            executeCount++;
                            break;
                        case TaskStatus.Finished:
                            executeCount++;
                            needRemoveTasks.Add(task.Name);
                            break;
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


