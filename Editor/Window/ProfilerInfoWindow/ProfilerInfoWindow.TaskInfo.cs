using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class ProfilerInfoWindow
    {

        private List<ProfilerTaskInfo> taskInfoList;
        private Vector2 taskInfoScrollPos;

        private void ClearTaskInfoView()
        {
            taskInfoList = null;
        }

        /// <summary>
        /// 绘制任务信息界面
        /// </summary>
        private void DrawTaskInfoView()
        {


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

                if (taskInfoList == null)
                {
                    return;
                }

                foreach (var profilerTaskInfo in taskInfoList)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(profilerTaskInfo.Name, GUILayout.Width(position.width / 2));
                        EditorGUILayout.LabelField(profilerTaskInfo.Type);
                        EditorGUILayout.LabelField(profilerTaskInfo.State.ToString());
                        EditorGUILayout.LabelField(profilerTaskInfo.Progress.ToString("0.00"));
                        EditorGUILayout.LabelField(profilerTaskInfo.MergedTaskCount.ToString());
                    }
                }

            }
        }
    }
}
