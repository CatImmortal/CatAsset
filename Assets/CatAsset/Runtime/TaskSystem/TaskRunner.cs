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
        public static int GUIDFactory = 0;

        /// <summary>
        /// 任务组列表
        /// </summary>
        private List<TaskGroup> taskGroups = new List<TaskGroup>();

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
        /// 单帧最大任务运行次数
        /// </summary>
        public int MaxRunCount { get; set; } = int.MaxValue;

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(ITask task,TaskPriority priority)
        {
            taskGroups[(int)priority].AddTask(task);
        }

        /// <summary>
        /// 轮询任务运行器
        /// </summary>
        public void Update()
        {
            //当前运行任务次数
            int curRanCount = 0;
            
            for (int i = taskGroups.Count - 1; i >= 0; i--)
            {
                TaskGroup group = taskGroups[i];
                
                group.PreRun();

                while (curRanCount < MaxRunCount && group.CanRun)
                {
                    if (group.Run())
                    {
                        //Run调用返回true 意味着需要增加curRanCount
                        curRanCount++;
                    }
                }
                
                group.PostRun();
            }
        }


    }
}

