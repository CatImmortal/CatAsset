using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源信息界面
    /// </summary>
    public class AssetInfoView : BaseProfilerInfoView
    {
        protected override List<string> GetColumns()
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

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new AssetInfoTreeView(State, Header);
        }

        public override void DrawInfoView(Rect position)
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
