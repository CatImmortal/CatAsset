using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace CatAsset.Editor
{
    public partial class AssetRuntimeInfoWindow
    {
        private bool isInitGroupInfoView;
        private Dictionary<string, GroupInfo> groupInfoDict;

        private void InitGroupInfoView()
        {
            isInitGroupInfoView = true;
            groupInfoDict = typeof(CatAssetManager).GetField("groupInfoDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, GroupInfo>;
        }

        private void DrawGroupInfoView()
        {
            if (!isInitGroupInfoView)
            {
                InitGroupInfoView();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("资源组");
                EditorGUILayout.LabelField("远端资源数");
                EditorGUILayout.LabelField("远端资源长度");
                EditorGUILayout.LabelField("本地资源数");
                EditorGUILayout.LabelField("本地资源长度");

            }

            foreach (KeyValuePair<string, GroupInfo> item in groupInfoDict)
            {
                DrawGroupInfo(item.Value);
            }
        }

        private void DrawGroupInfo(GroupInfo groupInfo)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(groupInfo.GroupName);
                EditorGUILayout.LabelField(groupInfo.remoteCount.ToString());
                EditorGUILayout.LabelField(GetByteDesc(groupInfo.remoteLength));
                EditorGUILayout.LabelField(groupInfo.localCount.ToString());
                EditorGUILayout.LabelField(GetByteDesc(groupInfo.localLength));
            }
        }
    }

}

