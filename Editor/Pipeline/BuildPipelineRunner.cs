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
        /// 参数名->参数
        /// </summary>
        private static readonly Dictionary<string, object> injectDict = new Dictionary<string, object>();

        /// <summary>
        /// 运行构建管线任务
        /// </summary>
        public static void Run(List<IBuildPipelineTask> tasks)
        {
            foreach (IBuildPipelineTask task in tasks)
            {
                if (task.Run() == TaskResult.Failed)
                {
                    Debug.LogError($"构建管线任务运行失败，当前任务:{task.GetType().Name}");
                    return;
                }
                Debug.Log($"构建管线任务运行成功，当前任务:{task.GetType().Name}");
            }
            Debug.Log("资源管线运行结束");
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 注入构建管线参数
        /// </summary>
        public static void InjectPipelineParam(string key, object param)
        {
            injectDict[key] = param;
        }
        
        /// <summary>
        /// 获取构建管线参数
        /// </summary>
        public static T GetPipelineParam<T>(string key)
        {
            if (!injectDict.TryGetValue(key,out object obj))
            {
                return default;
            }

            if (!(obj is T param))
            {
                throw new Exception($"GetParam调用失败，使用了{typeof(T).Name}类型来获取{obj.GetType().Name}类型的参数");
            }

            return param;
        }


    }
}