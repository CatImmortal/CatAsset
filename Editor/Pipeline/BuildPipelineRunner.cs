using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 构建管线运行器
    /// </summary>
    public static class BuildPipelineRunner
    {
        
        /// <summary>
        /// 参数类型->参数对象
        /// </summary>
        private static readonly Dictionary<Type, IBuildPipelineParam> paramDict =
            new Dictionary<Type, IBuildPipelineParam>();

        /// <summary>
        /// 运行构建管线任务
        /// </summary>
        public static TaskResult Run(List<IBuildPipelineTask> tasks)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                IBuildPipelineTask task = tasks[i];

                //输入参数
                BuildPipelineInjector.In(task);

                EditorUtility.DisplayProgressBar("构建管线运行中...", $"当前任务:{task.GetType().Name}", (i * 1.0f) / tasks.Count);

                if (task.Run() == TaskResult.Failed)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogError($"构建管线任务运行失败，当前任务:{task.GetType().Name}");
                    return TaskResult.Failed;
                }

                //输出参数
                BuildPipelineInjector.Out(task);

                Debug.Log($"构建管线任务运行成功，当前任务:{task.GetType().Name}");
            }

            EditorUtility.ClearProgressBar();
            Debug.Log("资源管线运行结束");
            return TaskResult.Success;
        }

        /// <summary>
        /// 注入构建管线参数
        /// </summary>
        public static void InjectParam(IBuildPipelineParam param)
        {
            paramDict[param.GetType()] = param;
        }

        /// <summary>
        /// 获取构建管线参数
        /// </summary>
        public static IBuildPipelineParam GetParam(Type key)
        {
            if (!paramDict.TryGetValue(key,out IBuildPipelineParam param))
            {
                return default;
            }

            return param;
        }
        
        /// <summary>
        /// 获取构建管线参数
        /// </summary>
        public static T GetParam<T>() where T : IBuildPipelineParam
        {
            return (T) GetParam(typeof(T));
        }


    }
}