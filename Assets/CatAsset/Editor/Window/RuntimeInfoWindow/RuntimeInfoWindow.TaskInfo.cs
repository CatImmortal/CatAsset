using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<TaskRunner, Dictionary<string, ITask>> allTasks;

        /// <summary>
        /// 初始化任务信息界面
        /// </summary>
        private void InitTaskInfoView()
        {
            isInitTaskInfoView = true;

            allTasks = typeof(TaskRunner).GetField("MainTaskDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as
                Dictionary<TaskRunner, Dictionary<string, ITask>>;

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

                foreach (KeyValuePair<TaskRunner, Dictionary<string, ITask>> pair in allTasks)
                {
                    foreach (KeyValuePair<string,ITask> item in pair.Value)
                    {
                        ITask task = item.Value;
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
