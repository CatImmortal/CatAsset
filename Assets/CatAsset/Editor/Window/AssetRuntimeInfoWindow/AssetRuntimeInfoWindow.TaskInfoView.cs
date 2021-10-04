using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class AssetRuntimeInfoWindow
    {
        private bool isInitTaskInfoView;
        private Vector2 taskInfoScrollPos;
        private Dictionary<string, BaseTask> taskDict;

        /// <summary>
        /// 初始化任务信息界面
        /// </summary>
        private void InitTaksInfoView()
        {
            isInitTaskInfoView = true;
            TaskExcutor taskExcutor = typeof(CatAssetManager).GetField("taskExcutor", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as TaskExcutor;
            taskDict = typeof(TaskExcutor).GetField("taskDict", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(taskExcutor) as Dictionary<string, BaseTask>;
        }

        /// <summary>
        /// 绘制任务信息界面
        /// </summary>
        private void DrawTaskInfoView()
        {
            if (!isInitTaskInfoView)
            {
                InitTaksInfoView();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("任务名称", GUILayout.Width(position.width / 2));
                EditorGUILayout.LabelField("任务类型");
                EditorGUILayout.LabelField("任务状态");
                EditorGUILayout.LabelField("任务进度");
            }

            using (EditorGUILayout.ScrollViewScope sv = new EditorGUILayout.ScrollViewScope(taskInfoScrollPos))
            {
                taskInfoScrollPos = sv.scrollPosition;
                foreach (KeyValuePair<string, BaseTask> item in taskDict)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        BaseTask task = item.Value;
                        EditorGUILayout.LabelField(task.Name,GUILayout.Width(position.width / 2));
                        EditorGUILayout.LabelField(task.GetType().Name);
                        EditorGUILayout.LabelField(task.State.ToString());
                        EditorGUILayout.LabelField(task.Progress.ToString("0.00"));
                    }

                }
            }

           
        }
    }
}

