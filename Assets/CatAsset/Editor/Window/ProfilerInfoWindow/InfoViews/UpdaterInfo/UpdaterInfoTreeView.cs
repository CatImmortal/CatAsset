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
    /// 更新器信息视图
    /// </summary>
    public class UpdaterInfoTreeView : BaseProfilerTreeView
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

            UpdatedCount,
            UpdatedLength,
            TotalCount,
            TotalLength,
            Speed,
            State
        }

        public UpdaterInfoTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {

        }

        public override void Reload(ProfilerInfo info)
        {
            ProfilerInfo = info;

            if (info.UpdaterInfoList.Count == 0)
            {
                return;
            }

            base.Reload(info);
        }

        public override bool CanShow()
        {
            return ProfilerInfo != null && ProfilerInfo.UpdaterInfoList.Count > 0;
        }

        public override void OnSortingChanged(MultiColumnHeader header)
        {
            if (header.sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = header.IsSortedAscending(header.sortedColumnIndex);

            ColumnType column = (ColumnType)header.sortedColumnIndex;

            IOrderedEnumerable<ProfilerUpdaterInfo> updaterOrdered = null;

            switch (column)
            {
                case ColumnType.Name:
                    updaterOrdered = ProfilerInfo.UpdaterInfoList.Order(info => info.Name, ascending);
                    break;
                case ColumnType.UpdatedCount:
                    updaterOrdered = ProfilerInfo.UpdaterInfoList.Order(info => info.UpdatedCount, ascending);
                    break;
                case ColumnType.UpdatedLength:
                    updaterOrdered = ProfilerInfo.UpdaterInfoList.Order(info => info.UpdatedLength, ascending);
                    break;
                case ColumnType.TotalCount:
                    updaterOrdered = ProfilerInfo.UpdaterInfoList.Order(info => info.TotalCount, ascending);
                    break;
                case ColumnType.TotalLength:
                    updaterOrdered = ProfilerInfo.UpdaterInfoList.Order(info => info.TotalLength, ascending);
                    break;
                case ColumnType.Speed:
                    updaterOrdered = ProfilerInfo.UpdaterInfoList.Order(info => info.Speed, ascending);
                    break;
                case ColumnType.State:
                    updaterOrdered = ProfilerInfo.UpdaterInfoList.Order(info => info.State, ascending);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (updaterOrdered != null)
            {
                ProfilerInfo.UpdaterInfoList = new List<ProfilerUpdaterInfo>(updaterOrdered);
                Reload();
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<ProfilerUpdaterInfo>()
            {
                id = 0, depth = -1, displayName = "Root",Data = null,
            };

            foreach (var updaterInfo in ProfilerInfo.UpdaterInfoList)
            {
                var groupNode = new TreeViewDataItem<ProfilerUpdaterInfo>()
                {
                    id = updaterInfo.Name.GetHashCode(), displayName = updaterInfo.Name, Data = updaterInfo,
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
            TreeViewDataItem<ProfilerUpdaterInfo> updaterItem = (TreeViewDataItem<ProfilerUpdaterInfo>)item;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};
            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    args.label = updaterItem.Data.Name;
                    base.RowGUI(args);
                    break;

                case ColumnType.UpdatedCount:
                    EditorGUI.LabelField(cellRect,updaterItem.Data.UpdatedCount.ToString(),centerStyle);
                    break;

                case ColumnType.UpdatedLength:
                    EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(updaterItem.Data.UpdatedLength),centerStyle);
                    break;

                case ColumnType.TotalCount:
                    EditorGUI.LabelField(cellRect,updaterItem.Data.TotalCount.ToString(),centerStyle);
                    break;

                case ColumnType.TotalLength:
                    EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(updaterItem.Data.TotalLength),centerStyle);
                    break;

                case ColumnType.Speed:
                    EditorGUI.LabelField(cellRect,$"{RuntimeUtil.GetByteLengthDesc(updaterItem.Data.Speed)}/S",centerStyle);
                    break;

                case ColumnType.State:
                    EditorGUI.LabelField(cellRect,updaterItem.Data.State.ToString(),centerStyle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }
    }
}
