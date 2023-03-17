using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务组
    /// </summary>
    public class TaskGroup
    {
        private static List<BaseTask> tempTaskList = new List<BaseTask>();

        /// <summary>
        /// 任务列表
        /// </summary>
        private List<BaseTask> mainTaskList = new List<BaseTask>();

        /// <summary>
        /// 当前任务索引
        /// </summary>
        private int curTaskIndex;

        /// <summary>
        /// 此任务组的优先级
        /// </summary>
        public TaskPriority Priority { get; }

        /// <summary>
        /// 任务组是否能运行
        /// </summary>
        public bool CanRun => curTaskIndex < tempTaskList.Count;

        public TaskGroup(TaskPriority priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(BaseTask task)
        {
            mainTaskList.Add(task);
            task.Group = this;
        }

        /// <summary>
        /// 移除任务
        /// </summary>
        public void RemoveTask(BaseTask task)
        {
            mainTaskList.Remove(task);
            task.Group = null;
        }

        /// <summary>
        /// 任务组运行前
        /// </summary>
        public void PreRun()
        {
            tempTaskList.Clear();
            curTaskIndex = 0;
            
            foreach (BaseTask task in mainTaskList)
            {
                tempTaskList.Add(task);
            }
        }

        /// <summary>
        /// 运行任务组
        /// </summary>
        public bool Run()
        {

            int index = curTaskIndex;
            curTaskIndex++;

            BaseTask task = tempTaskList[index];

            try
            {
                if (task.State == TaskState.Free)
                {
                    //运行空闲状态的任务
                    task.Run();
                }

                //轮询任务
                task.Update();
            }
            catch (Exception)
            {
                //任务出现异常 视为任务结束处理
                task.State = TaskState.Finished;
                Debug.LogError($"任务：{task.Name}，类型：{task.GetType().Name}，出现异常");
                throw;
            }
            finally
            {
                switch (task.State)
                {
                    case TaskState.Finished:
                        //任务运行结束 需要删除
                        RemoveTask(task);
                        TaskRunner.MainTaskDict[task.Owner][task.GetType()].Remove(task.Name);
                        ReferencePool.Release(task);
                        break;
                };
            }

            switch (task.State)
            {
                case TaskState.Running:
                case TaskState.Finished:
                    return true;
            }

            return false;

        }





    }
}
