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
    /// 任务信息TreeView
    /// </summary>
    public class TaskInfoTreeView : BaseMultiColumnTreeView<ProfilerInfo>
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

            Type,
            Priority,
            State,
            Progress,
            MergedTaskCount,
        }

        public TaskInfoTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {

        }

        public override bool CanShow()
        {
            return TreeViewData != null && TreeViewData.TaskInfoList.Count > 0;
        }


        protected override void OnSortingChanged(MultiColumnHeader header)
        {
            if (header.sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = header.IsSortedAscending(header.sortedColumnIndex);

            ColumnType column = (ColumnType)header.sortedColumnIndex;

            IOrderedEnumerable<ProfilerTaskInfo> taskOrdered = null;

            switch (column)
            {
                case ColumnType.Name:
                    taskOrdered = TreeViewData.TaskInfoList.Order(info => info.Name, ascending);
                    break;

                case ColumnType.Type:
                    taskOrdered = TreeViewData.TaskInfoList.Order(info => info.Type, ascending);
                    break;
                
                case ColumnType.Priority:
                    taskOrdered = TreeViewData.TaskInfoList.Order(info => info.Priority, ascending);
                    break;

                case ColumnType.State:
                    taskOrdered = TreeViewData.TaskInfoList.Order(info => info.State, ascending);
                    break;

                case ColumnType.Progress:
                    taskOrdered = TreeViewData.TaskInfoList.Order(info => info.Progress, ascending);
                    break;

                case ColumnType.MergedTaskCount:
                    taskOrdered = TreeViewData.TaskInfoList.Order(info => info.MergedTaskCount, ascending);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (taskOrdered != null)
            {
                TreeViewData.TaskInfoList = new List<ProfilerTaskInfo>(taskOrdered);
                Reload();
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<ProfilerTaskInfo>()
            {
                id = 0, depth = -1, displayName = "Root",Data = null,
            };

            foreach (var taskInfo in TreeViewData.TaskInfoList)
            {
                var taskNode = new TreeViewDataItem<ProfilerTaskInfo>()
                {
                    id = taskInfo.Name.GetHashCode(), displayName = taskInfo.Name, Data = taskInfo,
                };

                root.AddChild(taskNode);
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
            TreeViewDataItem<ProfilerTaskInfo> taskItem = (TreeViewDataItem<ProfilerTaskInfo>)item;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};
            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    args.label = taskItem.Data.Name;
                    base.RowGUI(args);
                    break;
                
                case ColumnType.Type:
                    EditorGUI.LabelField(cellRect,taskItem.Data.Type,centerStyle);
                    break;
                
                case ColumnType.Priority:
                    EditorGUI.LabelField(cellRect,((int)taskItem.Data.Priority).ToString(),centerStyle);
                    break;
                
                case ColumnType.State:
                    EditorGUI.LabelField(cellRect,taskItem.Data.State.ToString(),centerStyle);
                    break;
                
                case ColumnType.Progress:
                    EditorGUI.LabelField(cellRect,$"{(taskItem.Data.Progress * 100):0.00}%",centerStyle);
                    break;
                
                case ColumnType.MergedTaskCount:
                    EditorGUI.LabelField(cellRect,taskItem.Data.MergedTaskCount.ToString(),centerStyle);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }
    }
}
