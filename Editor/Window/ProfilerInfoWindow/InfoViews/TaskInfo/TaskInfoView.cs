using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 任务信息界面
    /// </summary>
    public class TaskInfoView : BaseProfilerInfoView
    {
        protected override List<string> GetColumns()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "类型",
                "状态",
                "进度",
                "已合并任务数"
            };

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new TaskInfoTreeView(State, Header);
        }
    }
}
