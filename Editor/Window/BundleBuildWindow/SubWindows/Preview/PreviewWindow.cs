using System.Collections.Generic;

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
                "长度",
                "压缩设置",
            };

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new PreviewTreeView(State, Header);
        }
    }
}