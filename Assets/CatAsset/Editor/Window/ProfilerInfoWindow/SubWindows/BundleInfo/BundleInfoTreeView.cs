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
    /// 资源包信息TreeView
    /// </summary>
    public class BundleInfoTreeView :  BaseMultiColumnTreeView<ProfilerInfo>
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
            /// 资源组
            /// </summary>
            Group,

            /// <summary>
            /// 内存中资源数
            /// </summary>
            InMemoryAssetCount,

            /// <summary>
            /// 引用中资源数
            /// </summary>
            ReferencingAssetCount,

            /// <summary>
            /// 长度
            /// </summary>
            Length,

            /// <summary>
            /// 内存中资源总长度
            /// </summary>
            InMemoryAssetLength,

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

        public BundleInfoTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state,multiColumnHeader)
        {
            useScrollView = true;
            showAlternatingRowBackgrounds = true;  //启用交替的行背景颜色，使每行的显示更清楚
            showBorder = true;  //在 TreeView 周围留出边距，以便显示一个细边框将其与其余内容分隔开
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }
        
        public override bool CanShow()
        {
            return TreeViewData != null && TreeViewData.BundleInfoList.Count > 0;
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
            IOrderedEnumerable<ProfilerBundleInfo> bundleOrdered = null;

            switch (column)
            {
                case ColumnType.Name:
                case ColumnType.Object:
                    foreach (var bundleInfo in TreeViewData.BundleInfoList)
                    {
                        assetOrdered = bundleInfo.InMemoryAssets.Order(info => info.Name, ascending);
                        bundleInfo.InMemoryAssets = new List<ProfilerAssetInfo>(assetOrdered);
                    }
                    bundleOrdered = TreeViewData.BundleInfoList.Order(info => info.RelativePath, ascending);
                    break;

                case ColumnType.Group:
                    bundleOrdered = TreeViewData.BundleInfoList.Order(info => info.Group, ascending);
                    break;

                case ColumnType.InMemoryAssetCount:
                    bundleOrdered = TreeViewData.BundleInfoList.Order(info => info.InMemoryAssets.Count, ascending);
                    break;

                case ColumnType.ReferencingAssetCount:
                    bundleOrdered = TreeViewData.BundleInfoList.Order(info => info.ReferencingAssetCount, ascending);
                    break;

                case ColumnType.Length:
                    foreach (var bundleInfo in TreeViewData.BundleInfoList)
                    {
                        assetOrdered = bundleInfo.InMemoryAssets.Order(info => info.Length, ascending);
                        bundleInfo.InMemoryAssets = new List<ProfilerAssetInfo>(assetOrdered);
                    }
                    bundleOrdered = TreeViewData.BundleInfoList.Order(info => info.Length, ascending);
                    break;

                case ColumnType.InMemoryAssetLength:
                    bundleOrdered = TreeViewData.BundleInfoList.Order(info => info.InMemoryAssetLength, ascending);
                    break;

                case ColumnType.UpStreamCount:
                    foreach (var bundleInfo in TreeViewData.BundleInfoList)
                    {
                        assetOrdered = bundleInfo.InMemoryAssets.Order(info => info.DependencyChain.UpStream.Count, ascending);
                        bundleInfo.InMemoryAssets = new List<ProfilerAssetInfo>(assetOrdered);
                    }
                    bundleOrdered = TreeViewData.BundleInfoList.Order(info => info.DependencyChain.UpStream.Count, ascending);
                    break;

                case ColumnType.DownStreamCount:
                    foreach (var bundleInfo in TreeViewData.BundleInfoList)
                    {
                        assetOrdered = bundleInfo.InMemoryAssets.Order(info => info.DependencyChain.DownStream.Count, ascending);
                        bundleInfo.InMemoryAssets = new List<ProfilerAssetInfo>(assetOrdered);
                    }
                    bundleOrdered = TreeViewData.BundleInfoList.Order(info => info.DependencyChain.DownStream.Count, ascending);
                    break;

                case ColumnType.OpenDependencyGraphView:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (bundleOrdered != null)
            {
                TreeViewData.BundleInfoList = new List<ProfilerBundleInfo>(bundleOrdered);
            }

            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<ProfilerBundleInfo>()
            {
                id = 0, depth = -1, displayName = "Root",Data = null,
            };

            foreach (var bundleInfo in TreeViewData.BundleInfoList)
            {
                var bundleNode = new TreeViewDataItem<ProfilerBundleInfo>()
                {
                    id = bundleInfo.RelativePath.GetHashCode(), displayName = $"{bundleInfo.RelativePath},{bundleInfo.Group}",Data = bundleInfo,
                };

                foreach (var assetInfo in bundleInfo.InMemoryAssets)
                {
                    var assetNode = new TreeViewDataItem<ProfilerAssetInfo>()
                    {
                        id = assetInfo.Name.GetHashCode(), displayName = assetInfo.Name, Data = assetInfo,
                    };

                    if (IsOnlyShowActiveLoad && assetInfo.RefCount == assetInfo.DependencyChain.DownStream.Count)
                    {
                        //只显示主动加载的资源时 如果资源引用计数与下游资源数相同 就是仅被依赖加载的资源 需要跳过
                        continue;
                    }

                    bundleNode.AddChild(assetNode);
                }

                if (IsOnlyShowActiveLoad && !bundleNode.hasChildren)
                {
                    //只显示主动加载的资源时 如果资源包里没有符合显示条件的资源 就跳过
                    continue;
                }
                root.AddChild(bundleNode);
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
            TreeViewDataItem<ProfilerBundleInfo> bundleItem = item as TreeViewDataItem<ProfilerBundleInfo>;
            TreeViewDataItem<ProfilerAssetInfo> assetItem = item as TreeViewDataItem<ProfilerAssetInfo>;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};
            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    if (bundleItem != null)
                    {
                        args.label = bundleItem.Data.RelativePath;
                    }
                    else
                    {
                        args.label = assetItem.Data.Name;
                    }
                    base.RowGUI(args);
                    break;

                case ColumnType.Object:
                    if (assetItem != null)
                    {
                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetItem.Data.Name);
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.ObjectField(cellRect, obj,typeof(Object),false);
                        EditorGUI.EndDisabledGroup();
                    }
                    break;

                case ColumnType.Group:
                    if (bundleItem != null)
                    {
                        EditorGUI.LabelField(cellRect,bundleItem.Data.Group,centerStyle);
                    }
                    break;

                case ColumnType.InMemoryAssetCount:
                    if (bundleItem != null)
                    {
                        EditorGUI.LabelField(cellRect,$"{bundleItem.Data.InMemoryAssets.Count}/{bundleItem.Data.TotalAssetCount}",centerStyle);
                    }
                    break;

                case ColumnType.ReferencingAssetCount:
                    if (bundleItem != null)
                    {
                        EditorGUI.LabelField(cellRect,$"{bundleItem.Data.ReferencingAssetCount}/{bundleItem.Data.TotalAssetCount}",centerStyle);
                    }
                    break;

                case ColumnType.Length:
                    ulong length = 0;
                    if (bundleItem != null)
                    {
                        length = bundleItem.Data.Length;
                    }
                    else
                    {
                        length = assetItem.Data.Length;
                    }
                    EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(length),centerStyle);
                    break;

                case ColumnType.InMemoryAssetLength:
                    if (bundleItem != null)
                    {
                        EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(bundleItem.Data.Length),centerStyle);
                    }
                    break;

                case ColumnType.UpStreamCount:
                    int count = 0;
                    if (bundleItem != null)
                    {
                        count = bundleItem.Data.DependencyChain.UpStream.Count;
                    }
                    else
                    {
                        count = assetItem.Data.DependencyChain.UpStream.Count;
                    }

                    EditorGUI.LabelField(cellRect, count.ToString(),centerStyle);
                    break;

                case ColumnType.DownStreamCount:
                    count = 0;
                    if (bundleItem != null)
                    {
                        count = bundleItem.Data.DependencyChain.DownStream.Count;
                    }
                    else
                    {
                        count = assetItem.Data.DependencyChain.DownStream.Count;
                    }

                    EditorGUI.LabelField(cellRect, count.ToString(),centerStyle);
                    break;

                case ColumnType.OpenDependencyGraphView:
                    if (GUI.Button(cellRect,"查看"))
                    {
                        if (bundleItem != null)
                        {
                            DependencyGraphViewWindow.Open<ProfilerBundleInfo,BundleNode>(bundleItem.Data);
                        }
                        else
                        {
                            DependencyGraphViewWindow.Open<ProfilerAssetInfo,AssetNode>(assetItem.Data);
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }
    }
}
