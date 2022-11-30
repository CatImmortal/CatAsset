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
        private TreeViewState updaterInfoTreeViewState;
        private UpdaterInfoTreeView updaterInfoTreeView;

        /// <summary>
        /// 初始化更新器信息树视图
        /// </summary>
        private void InitUpdaterInfoTreeView()
        {
            List<string> columnList = new List<string>()
            {
                "资源组",
                "已更新资源数",
                "已更新资源长度",
                "总资源数",
                "总资源长度",
                "下载速度",
                "状态"
            };

            var columns = CreateColumns(columnList);
            columns[0].minWidth = 400;

            var state = new MultiColumnHeaderState(columns);

            var header = new MultiColumnHeader(state);
            header.ResizeToFit();

            updaterInfoTreeViewState = new TreeViewState();
            updaterInfoTreeView = new UpdaterInfoTreeView(updaterInfoTreeViewState, header);
        }

        /// <summary>
        /// 绘制更新器信息界面
        /// </summary>
        private void DrawUpdaterInfoView()
        {
            if (updaterInfoTreeView.ProfilerInfo == null)
            {
                return;
            }

            updaterInfoTreeView.OnGUI(new Rect(0, 70, position.width, position.height - 70));
        }



    }
}
