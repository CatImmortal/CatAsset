using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 更新器信息窗口
    /// </summary>
    public class UpdaterInfoWindow : BaseTreeViewSubWindow
    {
        protected override List<string> GetColumns()
        {
            List<string> columnList = new List<string>()
            {
                "资源组",
                "已更新资源数",
                "已更新资源长度",
                "总资源数",
                "总资源长度",
                "下载速度",
                "状态"
            };

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new UpdaterInfoTreeView(State, Header);
        }
    }
}
