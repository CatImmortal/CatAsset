using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CatAsset;
using System.Reflection;

namespace CatAsset.Editor
{
    public partial class AssetRuntimeInfoWindow
    {
        private bool isInitUpdateInfoView;
        private static Dictionary<string, Updater> groupUpdaterDict;

        /// <summary>
        /// 初始化资源更新信息界面
        /// </summary>
        private void InitUpdateInfoView()
        {
            isInitUpdateInfoView = true;

            groupUpdaterDict = typeof(CatAssetUpdater).GetField("groupUpdaterDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, Updater>;
        }

        /// <summary>
        /// 绘制资源更新信息界面
        /// </summary>
        private void DrawUpdateInfoView()
        {
            if (!isInitUpdateInfoView)
            {
                InitUpdateInfoView();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("资源组");
                EditorGUILayout.LabelField("更新资源总数");
                EditorGUILayout.LabelField("更新资源长度");
                EditorGUILayout.LabelField("已更新资源数");
                EditorGUILayout.LabelField("已更新资源长度");
                EditorGUILayout.LabelField("状态");
            }
            foreach (KeyValuePair<string, Updater> item in groupUpdaterDict)
            {
                DrawUpdater(item.Value);
            }
        }

        /// <summary>
        /// 绘制资源更新器
        /// </summary>
        private void DrawUpdater(Updater updater)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(updater.UpdateGroup);

                EditorGUILayout.LabelField(updater.TotalCount.ToString());
                EditorGUILayout.LabelField(GetByteDesc(updater.TotalLength));
                EditorGUILayout.LabelField(updater.UpdatedCount.ToString());
                EditorGUILayout.LabelField(GetByteDesc(updater.UpdatedLength));
                EditorGUILayout.LabelField(updater.state.ToString());
            }
        }
    }

}

