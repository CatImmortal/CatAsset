using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源组信息窗口
    /// </summary>
    public class GroupInfoWindow : BaseTreeViewSubWindow
    {
        protected override List<string> GetColumns()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "本地资源列表",
                "状态",
                "本地资源数",
                "本地资源长度",
                "远端资源列表",
                "远端资源数",
                "远端资源长度"
            };

            return columnList;
        }
        
        protected override void ProcessColumns(MultiColumnHeaderState.Column[] columns)
        {
            columns[0].minWidth = 200;
            columns[1].minWidth = 400;
            columns[5].minWidth = 400;
        }

        protected override void CreateTreeView()
        {
            TreeView = new GroupInfoTreeView(State, Header);
        }
    }
}
