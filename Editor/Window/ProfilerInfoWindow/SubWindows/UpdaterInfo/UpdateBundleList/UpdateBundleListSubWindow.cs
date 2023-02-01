using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Editor
{
    public class UpdateBundleListSubWindow : BaseTreeViewSubWindow
    {
        protected override List<string> GetColumns()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "状态",
                "长度",
                "进度",
            };

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new UpdateBundleListTreeView(State, Header);
        }
        
        /// <inheritdoc/>
        public override void DrawSubWindow(Rect position)
        {
            if (!TreeView.CanShow())
            {
                return;
            }

            TreeView.OnGUI(new Rect(0, 0, position.width, position.height));
        }
    }
}