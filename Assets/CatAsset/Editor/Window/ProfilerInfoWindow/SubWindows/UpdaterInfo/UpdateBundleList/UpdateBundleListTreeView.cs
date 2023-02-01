using System;
using System.Collections.Generic;
using System.Linq;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    public class UpdateBundleListTreeView : BaseMultiColumnTreeView<List<ProfilerUpdateBundleInfo>>
    {
        /// <summary>
        /// 列类型
        /// </summary>
        private enum ColumnType
        {

            Name,
            State,
            Length,
            Progress,
        }
        
        public UpdateBundleListTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
        }

        public override bool CanShow()
        {
            return TreeViewData != null && TreeViewData.Count > 0;
        }

        protected override void OnSortingChanged(MultiColumnHeader header)
        {
            if (header.sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = header.IsSortedAscending(header.sortedColumnIndex);

            ColumnType column = (ColumnType)header.sortedColumnIndex;

            IOrderedEnumerable<ProfilerUpdateBundleInfo> updaterOrdered = null;

            switch (column)
            {
                case ColumnType.Name:
                    updaterOrdered = TreeViewData.Order(info => info.Name, ascending);
                    break;
                
                case ColumnType.State:
                    updaterOrdered = TreeViewData.Order(info => info.State, ascending);
                    break;
                
                case ColumnType.Length:
                    updaterOrdered = TreeViewData.Order(info => info.Length, ascending);
                    break;
                
                case ColumnType.Progress:
                    updaterOrdered = TreeViewData.Order(info => info.Progress, ascending);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (updaterOrdered != null)
            {
                TreeViewData = new List<ProfilerUpdateBundleInfo>(updaterOrdered);
                Reload();
            }
        }
        
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<ProfilerUpdateBundleInfo>()
            {
                id = 0, depth = -1, displayName = "Root",Data = null,
            };

            foreach (var updateBundleInfo in TreeViewData)
            {
                var updateBundleNode = new TreeViewDataItem<ProfilerUpdateBundleInfo>()
                {
                    id = updateBundleInfo.Name.GetHashCode(), displayName = updateBundleInfo.Name, Data = updateBundleInfo,
                };

                root.AddChild(updateBundleNode);
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
            TreeViewDataItem<ProfilerUpdateBundleInfo> updateBundleItem = (TreeViewDataItem<ProfilerUpdateBundleInfo>)item;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};

            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    args.label = updateBundleItem.Data.Name;
                    base.RowGUI(args);
                    break;
                
                case ColumnType.State:
                    EditorGUI.LabelField(cellRect,updateBundleItem.Data.State.ToString(),centerStyle);
                    break;
                
               case ColumnType.Length:
                   string updated = RuntimeUtil.GetByteLengthDesc(updateBundleItem.Data.UpdatedLength);
                   string length = RuntimeUtil.GetByteLengthDesc(updateBundleItem.Data.Length);
                   EditorGUI.LabelField(cellRect,$"{updated}/{length}",centerStyle);
                   break;
               
               case ColumnType.Progress:
                   EditorGUI.LabelField(cellRect,$"{(updateBundleItem.Data.Progress * 100):0.00}%",centerStyle);
                   break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }
    }
}