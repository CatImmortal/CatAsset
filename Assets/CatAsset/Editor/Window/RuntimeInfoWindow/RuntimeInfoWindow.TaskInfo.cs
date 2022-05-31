using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class RuntimeInfoWindow
    {
        private bool isInitTaskInfoView;
        private Vector2 taskInfoScrollPos;
        private List<List<ITask>> allRunningTasks;

        
        /// <summary>
        /// 初始化任务信息界面
        /// </summary>
        private void InitTaskInfoView()
        {
            isInitTaskInfoView = true;

            allRunningTasks = new List<List<ITask>>();
            AddRunningTasks("downloadTaskRunner");
            AddRunningTasks("loadTaskRunner");

        }

        /// <summary>
        /// 添加运行中任务列表
        /// </summary>
        private void AddRunningTasks(string fieldName)
        {
            TaskRunner taskRunner =
                typeof(CatAssetManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null) as TaskRunner;
            List<TaskGroup> groups =
                typeof(TaskRunner).GetField("taskGroups", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(taskRunner) as List<TaskGroup>;

            foreach (TaskGroup group in groups)
            {
                List<ITask> runningTasks = typeof(TaskGroup)
                    .GetField("runningTasks", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(group) as List<ITask>;
                
                allRunningTasks.Add(runningTasks);
            }
            
        }

        /// <summary>
        /// 绘制任务信息界面
        /// </summary>
        private void DrawTaskInfoView()
        {
            if (!isInitTaskInfoView)
            {
                InitTaskInfoView();
            }
            
            using (EditorGUILayout.ScrollViewScope sv = new EditorGUILayout.ScrollViewScope(taskInfoScrollPos))
            {
                taskInfoScrollPos = sv.scrollPosition;

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("任务名称", GUILayout.Width(position.width / 2));
                    EditorGUILayout.LabelField("任务类型");
                    EditorGUILayout.LabelField("任务状态");
                    EditorGUILayout.LabelField("任务进度");
                    EditorGUILayout.LabelField("已合并任务数");
                }
                
                foreach (List<ITask> runningTask in allRunningTasks)
                {
                    foreach (ITask task in runningTask)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(task.Name, GUILayout.Width(position.width / 2));
                            EditorGUILayout.LabelField(task.GetType().Name);
                            EditorGUILayout.LabelField(task.State.ToString());
                            EditorGUILayout.LabelField(task.Progress.ToString("0.00"));
                            EditorGUILayout.LabelField(task.MergedTaskCount.ToString());
                        }
                    }
                }
            }
        }
    }
}