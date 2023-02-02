using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Editor
{
    public class PreviewWindow : BaseTreeViewSubWindow
    {
        protected override List<string> GetColumns()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "Object",
                "类型",
                "资源组",
                "资源数",
                "预估长度",
                "压缩设置",
                "加密设置",
            };

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new PreviewTreeView(State, Header);
        }
        
        /// <inheritdoc/>
        public override void DrawSubWindow(Rect position)
        {
            if (!TreeView.CanShow())
            {
                return;
            }
            TreeView.OnGUI(new Rect(0, 60, position.width, position.height - 60));
        }
    }
}