using System.Collections.Generic;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;

namespace CatAsset.Editor
{
    public partial class RuntimeInfoWindow
    {
        private bool isInitGroupInfoView;
        private Dictionary<string, GroupInfo> groupInfoDict;

        /// <summary>
        /// 初始化资源组信息界面
        /// </summary>
        private void InitGroupInfoView()
        {
            isInitGroupInfoView = true;
            groupInfoDict = typeof(CatAssetDatabase).GetField("groupInfoDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, GroupInfo>;
        }

        /// <summary>
        /// 绘制资源组信息界面
        /// </summary>
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
                EditorGUILayout.LabelField(groupInfo.RemoteCount.ToString());
                EditorGUILayout.LabelField(Runtime.Util.GetByteLengthDesc(groupInfo.RemoteLength));
                EditorGUILayout.LabelField(groupInfo.LocalCount.ToString());
                EditorGUILayout.LabelField(Runtime.Util.GetByteLengthDesc(groupInfo.LocalLength));
            }
        }
    }
}