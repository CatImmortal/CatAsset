using System.Collections.Generic;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class ProfilerInfoWindow
    {
        private TreeViewState groupInfoTreeViewState;
        private GroupInfoTreeView groupInfoTreeView;

        /// <summary>
        /// 初始化资源组信息树视图
        /// </summary>
        private void InitGroupInfoTreeView()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "本地资源数",
                "本地资源长度",
                "远端资源数",
                "远端资源长度"
            };

            var columns = CreateColumns(columnList);
            columns[0].minWidth = 400;

            var state = new MultiColumnHeaderState(columns);

            var header = new MultiColumnHeader(state);
            header.ResizeToFit();

            groupInfoTreeViewState = new TreeViewState();
            groupInfoTreeView = new GroupInfoTreeView(groupInfoTreeViewState, header);
        }

        /// <summary>
        /// 绘制资源组信息界面
        /// </summary>
        private void DrawGroupInfoView()
        {
            if (groupInfoTreeView.ProfilerInfo == null)
            {
                return;
            }

            groupInfoTreeView.OnGUI(new Rect(0, 70, position.width, position.height - 70));
        }

    }
}
