using System;
using System.Collections.Generic;
using System.Linq;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源信息TreeView
    /// </summary>
    public class AssetInfoTreeView : BaseMultiColumnTreeView<ProfilerInfo>
    {
        /// <summary>
        /// 是否只显示主动加载的资源
        /// </summary>
        public bool IsOnlyShowActiveLoad = false;

        /// <summary>
        /// 列类型
        /// </summary>
        private enum ColumnType
        {
            /// <summary>
            /// 名称
            /// </summary>
            Name,

            /// <summary>
            /// 对象引用
            /// </summary>
            Object,

            /// <summary>
            /// 资源类型
            /// </summary>
            Type,
            
            /// <summary>
            /// 内存大小
            /// </summary>
            MemorySize,

            /// <summary>
            /// 资源包
            /// </summary>
            Bundle,
            
            /// <summary>
            /// 资源组
            /// </summary>
            Group,
            
            /// <summary>
            /// 引用计数
            /// </summary>
            RefCount,

            /// <summary>
            /// 上游节点数
            /// </summary>
            UpStreamCount,

            /// <summary>
            /// 下游节点数
            /// </summary>
            DownStreamCount,

            /// <summary>
            /// 查看依赖关系图
            /// </summary>
            OpenDependencyGraphView,
        }

        public AssetInfoTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {

        }

        public override bool CanShow()
        {
            return TreeViewData != null && TreeViewData.AssetInfoList.Count > 0;
        }

        protected override void OnSortingChanged(MultiColumnHeader header)
        {
            if (header.sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = header.IsSortedAscending(header.sortedColumnIndex);

            ColumnType column = (ColumnType)header.sortedColumnIndex;

            IOrderedEnumerable<ProfilerAssetInfo> assetOrdered = null;

            switch (column)
            {
                case ColumnType.Name:
                case ColumnType.Object:
                    assetOrdered = TreeViewData.AssetInfoList.Order(info => info.Name, ascending);
                    break;

                case ColumnType.Type:
                    assetOrdered = TreeViewData.AssetInfoList.Order(info => info.Type, ascending);
                    break;

                case ColumnType.Group:
                    assetOrdered = TreeViewData.AssetInfoList.Order(info => info.Group, ascending);
                    break;

                case ColumnType.Bundle:
                    assetOrdered = TreeViewData.AssetInfoList.Order(info => info.Bundle, ascending);
                    break;

                case ColumnType.MemorySize:
                    assetOrdered = TreeViewData.AssetInfoList.Order(info => info.MemorySize, ascending);
                    break;

                case ColumnType.RefCount:
                    assetOrdered = TreeViewData.AssetInfoList.Order(info => info.RefCount, ascending);
                    break;

                case ColumnType.UpStreamCount:
                    assetOrdered = TreeViewData.AssetInfoList.Order(info => info.DependencyChain.UpStream.Count, ascending);
                    break;

                case ColumnType.DownStreamCount:
                    assetOrdered = TreeViewData.AssetInfoList.Order(info => info.DependencyChain.DownStream.Count, ascending);
                    break;

                case ColumnType.OpenDependencyGraphView:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (assetOrdered != null)
            {
                TreeViewData.AssetInfoList = new List<ProfilerAssetInfo>(assetOrdered);
                Reload();
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<ProfilerAssetInfo>()
            {
                id = 0, depth = -1, displayName = "Root",Data = null,
            };

            foreach (var assetInfo in TreeViewData.AssetInfoList)
            {
                if (IsOnlyShowActiveLoad && assetInfo.RefCount <= assetInfo.DependencyChain.DownStream.Count)
                {
                    //只显示主动加载的资源时 如果资源引用计数<=下游资源数 就是仅被依赖加载的资源 需要跳过
                    continue;
                }
                
                var assetNode = new TreeViewDataItem<ProfilerAssetInfo>()
                {
                    id = assetInfo.Name.GetHashCode(), displayName = assetInfo.Name, Data = assetInfo,
                };

                root.AddChild(assetNode);
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
            TreeViewDataItem<ProfilerAssetInfo> assetItem = (TreeViewDataItem<ProfilerAssetInfo>)item;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};
            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    args.label = assetItem.Data.Name;
                    base.RowGUI(args);
                    break;

                case ColumnType.Object:
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetItem.Data.Name);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.ObjectField(cellRect, obj,typeof(Object),false);
                    EditorGUI.EndDisabledGroup();
                    break;

                case ColumnType.Type:
                    EditorGUI.LabelField(cellRect,assetItem.Data.Type,centerStyle);
                    break;

                case ColumnType.Group:
                    EditorGUI.LabelField(cellRect,assetItem.Data.Group,centerStyle);
                    break;

                case ColumnType.Bundle:
                    EditorGUI.LabelField(cellRect,assetItem.Data.Bundle,centerStyle);
                    break;

                case ColumnType.MemorySize:
                    EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(assetItem.Data.MemorySize),centerStyle);
                    break;

                case ColumnType.RefCount:
                    EditorGUI.LabelField(cellRect,assetItem.Data.RefCount.ToString(),centerStyle);
                    break;

                case ColumnType.UpStreamCount:
                    int count = assetItem.Data.DependencyChain.UpStream.Count;;
                    EditorGUI.LabelField(cellRect, count.ToString(),centerStyle);
                    break;

                case ColumnType.DownStreamCount:
                    count = assetItem.Data.DependencyChain.DownStream.Count;
                    EditorGUI.LabelField(cellRect, count.ToString(),centerStyle);
                    break;

                case ColumnType.OpenDependencyGraphView:
                    if (GUI.Button(cellRect,"查看"))
                    {
                        DependencyGraphViewWindow.Open<ProfilerAssetInfo,AssetNode>(assetItem.Data);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }
    }
}
