using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源信息窗口
    /// </summary>
    public class AssetInfoWindow : BaseTreeViewSubWindow
    {
        protected override List<string> GetColumns()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "Object",
                "加载耗时",
                "类型",
                "内存大小",
                "资源包",
                "资源组",
                "引用计数",
                "上游节点数",
                "下游节点数",
                "查看依赖关系图",
            };

            return columnList;
        }

        protected override void ProcessColumns(MultiColumnHeaderState.Column[] columns)
        {
            base.ProcessColumns(columns);

            columns[3].minWidth = 150;  //类型
            columns[5].minWidth = 400;  //资源包
        }

        protected override void CreateTreeView()
        {
            TreeView = new AssetInfoTreeView(State, Header);
        }

        public override void DrawSubWindow(Rect position)
        {
            if (!TreeView.CanShow())
            {
                return;
            }

            AssetInfoTreeView assetInfoTreeView = (AssetInfoTreeView)TreeView;

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
