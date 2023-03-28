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
        internal static readonly Dictionary<TaskRunner, Dictionary<Type, Dictionary<string, BaseTask>>> MainTaskDict =
            new Dictionary<TaskRunner, Dictionary<Type, Dictionary<string, BaseTask>>>();

        /// <summary>
        /// 任务组列表
        /// </summary>
        private readonly List<TaskGroup> taskGroups = new List<TaskGroup>();

        /// <summary>
        /// 当前已运行任务的计数器
        /// </summary>
        private int curRunCounter;
        
        public TaskRunner()
        {
            //优先级数量
            int priorityNum = Enum.GetNames(typeof(TaskPriority)).Length;

            for (int i = 0; i < priorityNum; i++)
            {
                //每个优先级创建一个对应任务组
                taskGroups.Add(new TaskGroup((TaskPriority)i));
            }
        }

        /// <summary>
        /// 同时最大任务运行数量
        /// </summary>
        public int MaxRunCount { get; set; } = 30;

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(BaseTask task, TaskPriority priority)
        {
            Type taskType = task.GetType();

            BaseTask mainTask = GetMainTask(taskType, task.Name);
            if (mainTask == null)
            {
                //没有重复的同名任务 直接作为主任务添加
                var nameDict = MainTaskDict[this][taskType];
                nameDict.Add(task.Name,task);
                taskGroups[(int)priority].AddTask(task);
                
                //将主任务作为已合并任务列表中的第一个
                task.MergeTask(task);
            }
            else
            {
                //合并同类型同名任务到主任务里
                mainTask.MergeTask(task);
                if ((int)priority > (int)mainTask.Group.Priority)
                {
                    //新合并的同名任务比主任务原本的优先级更高 则将主任务转移到更高优先级的任务组中
                    ChangePriority(mainTask,priority);
                }
            }
        }

        /// <summary>
        /// 变更主任务优先级
        /// </summary>
        public void ChangePriority(BaseTask mainTask,TaskPriority newPriority)
        {
            if (mainTask.Group.Priority == newPriority)
            {
                return;
            }
            Debug.Log($"任务{mainTask.Name}变更优先级：{mainTask.Group.Priority}->{newPriority}");
            mainTask.Group.RemoveTask(mainTask);
            taskGroups[(int)newPriority].AddTask(mainTask);
            mainTask.OnPriorityChanged();
         
        }

        /// <summary>
        /// 获取指定任务类型的主任务字典
        /// </summary>
        private Dictionary<string, BaseTask> GetMainTaskDict(Type taskType)
        {
            if (!MainTaskDict.TryGetValue(this,out var typeDict))
            {
                typeDict = new Dictionary<Type, Dictionary<string, BaseTask>>();
                MainTaskDict.Add(this,typeDict);
            }
            
            if (!typeDict.TryGetValue(taskType,out var nameDict))
            {
                nameDict = new Dictionary<string, BaseTask>();
                typeDict.Add(taskType,nameDict);
            }
            
            return nameDict;
        }

        /// <summary>
        /// 获取指定类型与名称的主任务
        /// </summary>
        public BaseTask GetMainTask(Type taskType, string name)
        {
            var nameDict = GetMainTaskDict(taskType);
            nameDict.TryGetValue(name, out var task);
            return task;
        }

        /// <summary>
        /// 任务运行器轮询前
        /// </summary>
        public void PreUpdate()
        {
            curRunCounter = 0;
            foreach (TaskGroup group in taskGroups)
            {
                group.PreRun();
            }
        }
        
        /// <summary>
        /// 轮询任务运行器
        /// </summary>
        public void Update(int priority)
        {
            //当前运行任务计数器
            TaskGroup group = taskGroups[priority];

            while (curRunCounter < MaxRunCount && group.CanRun)
            {
                if (group.Run())
                {
                    //Run调用返回true 意味着需要增加计数器
                    curRunCounter++;
                }
            }
        }


    }
}

