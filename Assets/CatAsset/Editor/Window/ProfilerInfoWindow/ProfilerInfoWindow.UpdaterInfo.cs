using System.Collections.Generic;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;

namespace CatAsset.Editor
{
    public partial class ProfilerInfoWindow
    {
        private List<ProfilerUpdaterInfo> updaterInfoList;

        private void ClearUpdaterInfoView()
        {
            updaterInfoList = null;
        }

        /// <summary>
        /// 绘制更新器信息界面
        /// </summary>
        private void DrawUpdaterInfoView()
        {

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("资源组");
                EditorGUILayout.LabelField("资源包总数");
                EditorGUILayout.LabelField("资源包长度");
                EditorGUILayout.LabelField("已更新资源包总数");
                EditorGUILayout.LabelField("已更新资源长度");
                EditorGUILayout.LabelField("下载速度");
                EditorGUILayout.LabelField("状态");
            }

            if (updaterInfoList == null)
            {
                return;
            }

            foreach (var profilerUpdaterInfo in updaterInfoList)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(profilerUpdaterInfo.Name);

                    EditorGUILayout.LabelField(profilerUpdaterInfo.TotalCount.ToString());
                    EditorGUILayout.LabelField(RuntimeUtil.GetByteLengthDesc((long)profilerUpdaterInfo.TotalLength));
                    EditorGUILayout.LabelField(profilerUpdaterInfo.UpdatedCount.ToString());
                    EditorGUILayout.LabelField(RuntimeUtil.GetByteLengthDesc((long)profilerUpdaterInfo.UpdatedLength));
                    EditorGUILayout.LabelField($"{RuntimeUtil.GetByteLengthDesc((long)profilerUpdaterInfo.Speed)}/S");
                    EditorGUILayout.LabelField(profilerUpdaterInfo.State.ToString());
                }
            }
        }


    }
}
