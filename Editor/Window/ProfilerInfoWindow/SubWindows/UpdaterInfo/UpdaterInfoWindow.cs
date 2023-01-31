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
                "状态",
                "待更新资源数",
                "待更新资源长度",
                "更新中资源数",
                "更新中资源长度",
                "已更新资源数",
                "已更新资源长度",
                "总资源数",
                "总资源长度",
                "已下载字节数",
                "下载速度",
            };

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new UpdaterInfoTreeView(State, Header);
        }
    }
}
