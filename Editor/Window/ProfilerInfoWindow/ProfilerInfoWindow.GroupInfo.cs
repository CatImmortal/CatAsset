using System.Collections.Generic;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;

namespace CatAsset.Editor
{
    public partial class ProfilerInfoWindow
    {
        private List<ProfilerGroupInfo> groupInfoList;

        private void ClearGroupInfoView()
        {
            groupInfoList = null;
        }

        /// <summary>
        /// 绘制资源组信息界面
        /// </summary>
        private void DrawGroupInfoView()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("资源组");
                EditorGUILayout.LabelField("远端资源包数");
                EditorGUILayout.LabelField("远端资源包长度");
                EditorGUILayout.LabelField("本地资源包数");
                EditorGUILayout.LabelField("本地资源包长度");

            }

            if (groupInfoList == null)
            {
                return;
            }

            foreach (var profilerGroupInfo in groupInfoList)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(profilerGroupInfo.Name);
                    EditorGUILayout.LabelField(profilerGroupInfo.RemoteCount.ToString());
                    EditorGUILayout.LabelField(RuntimeUtil.GetByteLengthDesc(profilerGroupInfo.RemoteLength));
                    EditorGUILayout.LabelField(profilerGroupInfo.LocalCount.ToString());
                    EditorGUILayout.LabelField(RuntimeUtil.GetByteLengthDesc(profilerGroupInfo.LocalLength));
                }
            }


        }


    }
}
