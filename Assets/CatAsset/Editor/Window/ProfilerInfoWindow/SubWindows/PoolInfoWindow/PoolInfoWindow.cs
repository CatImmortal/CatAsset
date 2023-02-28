using System.Collections.Generic;

namespace CatAsset.Editor
{
    /// <summary>
    /// 对象池信息窗口
    /// </summary>
    public class PoolInfoWindow : BaseTreeViewSubWindow
    {
        protected override List<string> GetColumns()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "对象池失效时间",
                "对象失效时间",
                "空闲时间",
                "总对象数",
                "已使用对象数",
                "未使用对象数",
                
                "是否被锁定"
            };

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new PoolInfoTreeView(State, Header);
        }
    }
}