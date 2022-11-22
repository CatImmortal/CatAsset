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
        private Vector2 taskInfoScrollPos;
        private Dictionary<TaskRunner, Dictionary<string, BaseTask>> allTasks;

        /// <summary>
        /// 初始化任务信息界面
        /// </summary>
        private void InitTaskInfoView()
        {
            allTasks = typeof(TaskRunner).GetField("MainTaskDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as
                Dictionary<TaskRunner, Dictionary<string, BaseTask>>;
        }

        /// <summary>
        /// 绘制任务信息界面
        /// </summary>
        private void DrawTaskInfoView()
        {
            if (allTasks == null)
            {
                return;
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

                foreach (KeyValuePair<TaskRunner, Dictionary<string, BaseTask>> pair in allTasks)
                {
                    foreach (KeyValuePair<string,BaseTask> item in pair.Value)
                    {
                        BaseTask task = item.Value;
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
