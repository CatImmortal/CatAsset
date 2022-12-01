using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包信息界面
    /// </summary>
    public class BundleInfoView : BaseProfilerInfoView
    {
        protected override List<string> GetColumns()
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

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new BundleInfoTreeView(State, Header);
        }

        public override void DrawInfoView(Rect position)
        {
            if (!TreeView.CanShow())
            {
                return;
            }

            BundleInfoTreeView bundleInfoTreeView = (BundleInfoTreeView)TreeView;

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
