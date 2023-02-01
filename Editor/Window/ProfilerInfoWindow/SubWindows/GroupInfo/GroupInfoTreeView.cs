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
            Name,

            LocalBundles,
            State,
            LocalCount,
            LocalLength,
            
            RemoteBundles,
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
            IOrderedEnumerable<ProfilerGroupInfo.BundleInfo> bundleOrdered = null;

            switch (column)
            {
                case ColumnType.Name:
                    groupOrdered = TreeViewData.GroupInfoList.Order(info => info.Name, ascending);
                    break;
                
                case ColumnType.State:
                    foreach (var groupInfo in TreeViewData.GroupInfoList)
                    {
                        bundleOrdered = groupInfo.RemoteBundles.Order(info => info.State, ascending);
                        groupInfo.RemoteBundles = new List<ProfilerGroupInfo.BundleInfo>(bundleOrdered);
                    }
                    Reload();
                    break;
                
                case ColumnType.LocalCount:
                    groupOrdered = TreeViewData.GroupInfoList.Order(info => info.LocalCount, ascending);
                    break;
                
                case ColumnType.LocalLength:
                    groupOrdered = TreeViewData.GroupInfoList.Order(info => info.LocalLength, ascending);
                    break;
                
                case ColumnType.LocalBundles:
                case ColumnType.RemoteBundles:
                    foreach (var groupInfo in TreeViewData.GroupInfoList)
                    {
                        bundleOrdered = groupInfo.RemoteBundles.Order(info => info.Name, ascending);
                        groupInfo.RemoteBundles = new List<ProfilerGroupInfo.BundleInfo>(bundleOrdered);
                    }
                    Reload();
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

                foreach (ProfilerGroupInfo.BundleInfo info in groupInfo.RemoteBundles)
                {
                    var bundleNode = new TreeViewDataItem<ProfilerGroupInfo.BundleInfo>()
                    {
                        id = info.Name.GetHashCode(), displayName = info.Name, Data = info,
                    };
                    groupNode.AddChild(bundleNode);
                }

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
            var groupItem = item as TreeViewDataItem<ProfilerGroupInfo>;
            var bundleItem = item as TreeViewDataItem<ProfilerGroupInfo.BundleInfo>;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};

            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    if (groupItem != null)
                    {
                        args.label = groupItem.Data.Name;
                    }
                    else
                    {
                        args.label = string.Empty;
                    }
                    base.RowGUI(args);
                    break;
                
                case ColumnType.LocalBundles:
                    if (bundleItem != null && bundleItem.Data.State != BundleRuntimeInfo.State.InRemote)
                    {
                        EditorGUI.LabelField(cellRect,bundleItem.Data.Name,centerStyle);
                    }
                    break;
                
                case ColumnType.State:
                    if (bundleItem != null && bundleItem.Data.State != BundleRuntimeInfo.State.InRemote)
                    {
                        EditorGUI.LabelField(cellRect,bundleItem.Data.State.ToString(),centerStyle);
                    }
                    break;
                
                case ColumnType.LocalCount:
                    if (groupItem != null)
                    {
                        EditorGUI.LabelField(cellRect,groupItem.Data.LocalCount.ToString(),centerStyle);
                    }
                    break;
                
                case ColumnType.LocalLength:
                    ulong length = 0;
                    if (groupItem != null)
                    {
                        length = groupItem.Data.LocalLength;
                    }
                    else
                    {
                        length = bundleItem.Data.Length;
                    }
                    EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(length),centerStyle);
                    break;
                
                case ColumnType.RemoteBundles:
                    if (bundleItem != null)
                    {
                        EditorGUI.LabelField(cellRect,bundleItem.Data.Name,centerStyle);
                    }
                    break;
                
                case ColumnType.RemoteCount:
                    if (groupItem != null)
                    {
                        EditorGUI.LabelField(cellRect,groupItem.Data.RemoteCount.ToString(),centerStyle);
                    }
                    break;
                
                case ColumnType.RemoteLength:
                    length = 0;
                    if (groupItem != null)
                    {
                        length = groupItem.Data.RemoteLength;
                    }
                    else
                    {
                        length = bundleItem.Data.Length;
                    }
                    EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(length),centerStyle);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }
    }
}
