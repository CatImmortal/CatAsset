using System;
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
        private TreeViewState bundleInfoTreeViewState;
        private BundleInfoTreeView bundleInfoTreeView;

        /// <summary>
        /// 初始化资源包信息树视图
        /// </summary>
        private void InitBundleInfoTreeView()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "Object",
                "资源组",
                "内存中资源数",
                "引用中资源数",
                "长度",
                "内存中资源总长度",
                "上游节点数",
                "下游节点数",
                "查看依赖关系图",
            };

            var columns = CreateColumns(columnList);
            columns[0].minWidth = 400;

            var state = new MultiColumnHeaderState(columns);

            var header = new MultiColumnHeader(state);
            header.ResizeToFit();

            bundleInfoTreeViewState = new TreeViewState();
            bundleInfoTreeView = new BundleInfoTreeView(bundleInfoTreeViewState, header);
        }



        /// <summary>
        /// 绘制资源包信息界面
        /// </summary>
        private void DrawBundleInfoView()
        {
            if (profilerPlayer.IsEmpty)
            {
                return;
            }

            bool toggleValue = EditorGUI.ToggleLeft(new Rect(0, 50, 150, 20), "只显示主动加载的资源", bundleInfoTreeView.IsOnlyShowActiveLoad);
            if (bundleInfoTreeView.IsOnlyShowActiveLoad != toggleValue)
            {
                bundleInfoTreeView.IsOnlyShowActiveLoad = toggleValue;
                bundleInfoTreeView.Reload();
            }
            bundleInfoTreeView.OnGUI(new Rect(0, 70, position.width, position.height - 70));
        }


    }
}

