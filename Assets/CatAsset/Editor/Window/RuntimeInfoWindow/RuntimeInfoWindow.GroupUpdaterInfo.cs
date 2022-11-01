using System.Collections.Generic;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;

namespace CatAsset.Editor
{
    public partial class RuntimeInfoWindow
    {
        private bool isInitGroupUpdaterInfoView;
        private static Dictionary<string, GroupUpdater> groupUpdaterDict;

        /// <summary>
        /// 初始化资源组更新器信息界面
        /// </summary>
        private void InitGroupUpdaterInfoView()
        {
            isInitGroupUpdaterInfoView = true;

            groupUpdaterDict = typeof(CatAssetUpdater).GetField(nameof(groupUpdaterDict), BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, GroupUpdater>;
        }

        /// <summary>
        /// 绘制资源组更新器信息界面
        /// </summary>
        private void DrawGroupUpdaterInfoView()
        {
            if (!isInitGroupUpdaterInfoView)
            {
                InitGroupUpdaterInfoView();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("资源组");
                EditorGUILayout.LabelField("资源包总数");
                EditorGUILayout.LabelField("资源包长度");
                EditorGUILayout.LabelField("已更新资源包总数");
                EditorGUILayout.LabelField("已更新资源长度");
                EditorGUILayout.LabelField("状态");
            }
            foreach (KeyValuePair<string, GroupUpdater> item in groupUpdaterDict)
            {
                DrawGroupUpdater(item.Value);
            }
        }

        /// <summary>
        /// 绘制资源更新器
        /// </summary>
        private void DrawGroupUpdater(GroupUpdater updater)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(updater.GroupName);

                EditorGUILayout.LabelField(updater.TotalCount.ToString());
                EditorGUILayout.LabelField(RuntimeUtil.GetByteLengthDesc(updater.TotalLength));
                EditorGUILayout.LabelField(updater.UpdatedCount.ToString());
                EditorGUILayout.LabelField(RuntimeUtil.GetByteLengthDesc(updater.UpdatedLength));
                EditorGUILayout.LabelField(updater.State.ToString());
            }
        }
    }
}