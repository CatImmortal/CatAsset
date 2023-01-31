using System;
using System.Collections.Generic;
using System.Linq;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源组信息TreeView
    /// </summary>
    public class GroupInfoTreeView : BaseMultiColumnTreeView<ProfilerInfo>
    {
        /// <summary>
        /// 列类型
        /// </summary>
        private enum ColumnType
        {
            /// <summary>
            /// 名称
            /// </summary>
            Name,

            LocalCount,
            LocalLength,
            RemoteCount,
            RemoteLength,

        }

        public GroupInfoTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {

        }

        public override bool CanShow()
        {
            return TreeViewData != null && TreeViewData.GroupInfoList.Count > 0;
        }

        protected override void OnSortingChanged(MultiColumnHeader header)
        {
            if (header.sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = header.IsSortedAscending(header.sortedColumnIndex);

            ColumnType column = (ColumnType)header.sortedColumnIndex;

            IOrderedEnumerable<ProfilerGroupInfo> groupOrdered = null;

            switch (column)
            {
                case ColumnType.Name:
                    groupOrdered = TreeViewData.GroupInfoList.Order(info => info.Name, ascending);
                    break;

                case ColumnType.LocalCount:
                    groupOrdered = TreeViewData.GroupInfoList.Order(info => info.LocalCount, ascending);
                    break;

                case ColumnType.LocalLength:
                    groupOrdered = TreeViewData.GroupInfoList.Order(info => info.LocalLength, ascending);
                    break;

                case ColumnType.RemoteCount:
                    groupOrdered = TreeViewData.GroupInfoList.Order(info => info.RemoteCount, ascending);
                    break;

                case ColumnType.RemoteLength:
                    groupOrdered = TreeViewData.GroupInfoList.Order(info => info.RemoteLength, ascending);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (groupOrdered != null)
            {
                TreeViewData.GroupInfoList = new List<ProfilerGroupInfo>(groupOrdered);
                Reload();
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<ProfilerGroupInfo>()
            {
                id = 0, depth = -1, displayName = "Root",Data = null,
            };

            foreach (var groupInfo in TreeViewData.GroupInfoList)
            {
                var groupNode = new TreeViewDataItem<ProfilerGroupInfo>()
                {
                    id = groupInfo.Name.GetHashCode(), displayName = groupInfo.Name, Data = groupInfo,
                };

                root.AddChild(groupNode);
            }
            SetupDepthsFromParentsAndChildren(root);

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {

            for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
            {
                CellGUI(args.GetCellRect(i), args.item, (ColumnType)args.GetColumn(i), ref args);
            }
        }

        /// <summary>
        /// 绘制指定行每一列的内容
        /// </summary>
        private void CellGUI(Rect cellRect, TreeViewItem item, ColumnType column, ref RowGUIArgs args)
        {
            TreeViewDataItem<ProfilerGroupInfo> groupItem = (TreeViewDataItem<ProfilerGroupInfo>)item;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};
            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    args.label = groupItem.Data.Name;
                    base.RowGUI(args);
                    break;

                case ColumnType.LocalCount:
                    if (GUI.Button(cellRect, groupItem.Data.LocalCount.ToString()))
                    {
                       GroupBundleListWindow.Open($"{groupItem.Data.Name}组本地资源包列表",groupItem.Data.LocalBundles);
                    }
                    break;

                case ColumnType.LocalLength:
                    EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(groupItem.Data.LocalLength),centerStyle);
                    break;

                case ColumnType.RemoteCount:
                    if (GUI.Button(cellRect, groupItem.Data.RemoteCount.ToString()))
                    {
                        GroupBundleListWindow.Open($"{groupItem.Data.Name}组远端资源包列表",groupItem.Data.RemoteBundles);
                    }
                    break;

                case ColumnType.RemoteLength:
                    EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(groupItem.Data.RemoteLength),centerStyle);
                    break;



                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }
    }
}
