using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务运行器
    /// </summary>
    public class TaskRunner
    {
        /// <summary>
        /// 任务ID工厂
        /// </summary>
        internal static int TaskIDFactory = 0;

        /// <summary>
        /// 任务运行器->(任务名->主任务)
        /// </summary>
        internal static readonly Dictionary<TaskRunner, Dictionary<string, BaseTask>> MainTaskDict =
            new Dictionary<TaskRunner, Dictionary<string, BaseTask>>();

        /// <summary>
        /// 任务组列表
        /// </summary>
        private readonly List<TaskGroup> taskGroups = new List<TaskGroup>();

        public TaskRunner()
        {
            //优先级数量
            int priorityNum = Enum.GetNames(typeof(TaskPriority)).Length;

            for (int i = 0; i < priorityNum; i++)
            {
                //按优先级创建任务组
                taskGroups.Add(new TaskGroup((TaskPriority)i));
            }
        }

        /// <summary>
        /// 单帧最大任务运行数量
        /// </summary>
        public int MaxRunCount { get; set; } = 30;

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(BaseTask task, TaskPriority priority)
        {
            if (!MainTaskDict.TryGetValue(task.Owner,out Dictionary<string, BaseTask> dict))
            {
                dict = new Dictionary<string, BaseTask>();
                MainTaskDict.Add(task.Owner,dict);
            }

            if (dict.TryGetValue(task.Name,out BaseTask mainTask))
            {
                //合并同名任务到主任务里
                mainTask.MergeTask(task);

                if ((int)priority > (int)mainTask.Group.Priority)
                {
                    //新合并的同名任务比主任务优先级更高 则将主任务转移到更高优先级的任务组中
                    ChangePriority(mainTask,priority);
                }
            }
            else
            {
                dict.Add(task.Name,task);
                taskGroups[(int)priority].AddTask(task);
            }
        }

        /// <summary>
        /// 变更优先级
        /// </summary>
        private void ChangePriority(BaseTask task, TaskPriority newPriority)
        {
            if (task.Group.Priority == newPriority)
            {
                return;
            }

            Debug.Log($"任务{task.Name}变更优先级：{task.Group.Priority}->{newPriority}");
            task.Group.RemoveTask(task);
            taskGroups[(int)newPriority].AddTask(task);
        }

        /// <summary>
        /// 轮询任务运行器
        /// </summary>
        public void Update()
        {
            //当前运行任务计数器
            int curRunCounter = 0;

            for (int i = taskGroups.Count - 1; i >= 0; i--)
            {
                TaskGroup group = taskGroups[i];

                group.PreRun();

                while (curRunCounter < MaxRunCount && group.CanRun)
                {
                    if (group.Run())
                    {
                        //Run调用返回true 意味着需要增加计数器
                        curRunCounter++;
                    }
                }

                group.PostRun();
            }
        }


    }
}

