using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class ProfilerInfoWindow
    {
        private TreeViewState assetInfoTreeViewState;
        private AssetInfoTreeView assetInfoTreeView;

        /// <summary>
        /// 初始化资源信息树视图
        /// </summary>
        private void InitAssetInfoTreeView()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "Object",
                "类型",
                "资源组",
                "资源包",
                "长度",
                "引用计数",
                "上游节点数",
                "下游节点数",
                "查看依赖关系图",
            };

            var columns = CreateColumns(columnList);
            columns[0].minWidth = 400;
            columns[4].minWidth = 400;

            var state = new MultiColumnHeaderState(columns);

            var header = new MultiColumnHeader(state);
            header.ResizeToFit();

            assetInfoTreeViewState = new TreeViewState();
            assetInfoTreeView = new AssetInfoTreeView(assetInfoTreeViewState, header);
        }

        /// <summary>
        /// 绘制资源信息界面
        /// </summary>
        private void DrawAssetInfoView()
        {
            if (profilerPlayer.IsEmpty)
            {
                return;
            }

            bool toggleValue = EditorGUI.ToggleLeft(new Rect(0, 50, 150, 20), "只显示主动加载的资源", assetInfoTreeView.IsOnlyShowActiveLoad);
            if (assetInfoTreeView.IsOnlyShowActiveLoad != toggleValue)
            {
                assetInfoTreeView.IsOnlyShowActiveLoad = toggleValue;
                assetInfoTreeView.Reload();
            }
            assetInfoTreeView.OnGUI(new Rect(0, 70, position.width, position.height - 70));
        }
    }
}
