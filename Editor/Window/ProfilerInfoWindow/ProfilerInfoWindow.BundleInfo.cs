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
                "引用计数",
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
        /// 绘制运行时信息界面
        /// </summary>
        private void DrawBundleInfoView()
        {
            if (profilerInfo == null ||  profilerInfo.BundleInfoList.Count == 0)
            {
                return;
            }

            bundleInfoTreeView.OnGUI(new Rect(0, 60, position.width, position.height));
        }


    }
}

