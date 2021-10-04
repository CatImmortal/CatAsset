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
        private static FieldInfo updaterFi;

        /// <summary>
        /// 初始化资源更新信息界面
        /// </summary>
        private void InitUpdateInfoView()
        {
            isInitUpdateInfoView = true;

            groupUpdaterDict = typeof(CatAssetUpdater).GetField("groupUpdaterDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, Updater>;
            updaterFi = typeof(CatAssetUpdater).GetField("updater", BindingFlags.NonPublic | BindingFlags.Static);
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
                EditorGUILayout.LabelField("是否暂停中");
            }
            object value = updaterFi.GetValue(null);
            if (value != null)
            {
                DrawUpdater((Updater)value);
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
                if (string.IsNullOrEmpty(updater.UpdateGroup))
                {
                    EditorGUILayout.LabelField("无");
                }
                else
                {
                    EditorGUILayout.LabelField(updater.UpdateGroup);
                }

                EditorGUILayout.LabelField(updater.totalCount.ToString());
                //if (GUILayout.Button("Log"))
                //{
                //    string log = string.Empty;
                //    foreach (AssetBundleManifestInfo item in updater.UpdateList)
                //    {
                //        log += item.AssetBundleName + "\n";
                //    }

                //    Debug.Log(log);
                //}

                EditorGUILayout.LabelField(GetByteDesc(updater.totalLength));
                EditorGUILayout.LabelField(updater.updatedCount.ToString());
                EditorGUILayout.LabelField(GetByteDesc(updater.updatedLength));
                EditorGUILayout.LabelField(updater.paused.ToString());
            }
        }
    }

}

