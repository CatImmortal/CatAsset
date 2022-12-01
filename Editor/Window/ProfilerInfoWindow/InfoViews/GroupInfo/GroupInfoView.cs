using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源组信息界面
    /// </summary>
    public class GroupInfoView : BaseProfilerInfoView
    {
        protected override List<string> GetColumns()
        {
            List<string> columnList = new List<string>()
            {
                "名称",
                "本地资源数",
                "本地资源长度",
                "远端资源数",
                "远端资源长度"
            };

            return columnList;
        }

        protected override void CreateTreeView()
        {
            TreeView = new GroupInfoTreeView(State, Header);
        }
    }
}
