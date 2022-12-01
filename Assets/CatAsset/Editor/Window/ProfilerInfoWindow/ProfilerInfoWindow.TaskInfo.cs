using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class ProfilerInfoWindow
    {

        private TreeViewState taskInfoTreeViewState;
        private TaskInfoTreeView taskInfoTreeView;

        /// <summary>
        /// 初始化任务信息树视图
        /// </summary>
        private void InitTaskInfoTreeView()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "类型",
                "状态",
                "进度",
                "已合并任务数"
            };

            var columns = CreateColumns(columnList);
            columns[0].minWidth = 400;

            var state = new MultiColumnHeaderState(columns);

            var header = new MultiColumnHeader(state);
            header.ResizeToFit();

            taskInfoTreeViewState = new TreeViewState();
            taskInfoTreeView = new TaskInfoTreeView(taskInfoTreeViewState, header);
        }

        /// <summary>
        /// 绘制任务信息界面
        /// </summary>
        private void DrawTaskInfoView()
        {
            if (!taskInfoTreeView.CanShow())
            {
                return;
            }

            taskInfoTreeView.OnGUI(new Rect(0, 70, position.width, position.height - 70));
        }
    }
}
