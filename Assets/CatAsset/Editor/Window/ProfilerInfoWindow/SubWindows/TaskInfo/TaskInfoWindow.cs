using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 任务信息窗口
    /// </summary>
    public class TaskInfoWindow : BaseTreeViewSubWindow
    {
        protected override List<string> GetColumns()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "类型",
                "优先级",
                "状态",
                "子状态",
                "进度",
                "已合并任务数"
            };

            return columnList;
        }
        
        protected override void ProcessColumns(MultiColumnHeaderState.Column[] columns)
        {
            base.ProcessColumns(columns);

            columns[1].minWidth = 150;  //类型
            columns[4].minWidth = 150;  //子状态
        }

        protected override void CreateTreeView()
        {
            TreeView = new TaskInfoTreeView(State, Header);
        }
    }
}
