﻿using System;
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
    /// 资源包预览TreeView
    /// </summary>
    public class PreviewTreeView : BaseMultiColumnTreeView<BundleBuildConfigSO>
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

            /// <summary>
            /// 对象引用
            /// </summary>
            Object,

            /// <summary>
            /// 资源组
            /// </summary>
            Group,
            
            /// <summary>
            /// 资源数
            /// </summary>
            AssetCount,
            
            /// <summary>
            /// 长度
            /// </summary>
            Length,
        }
        
        public PreviewTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            Reload(BundleBuildConfigSO.Instance);
        }
        
        public override bool CanShow()
        {
            return TreeViewData != null && TreeViewData.Bundles.Count > 0;
        }

        protected override void OnSortingChanged(MultiColumnHeader header)
        {
            if (header.sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = header.IsSortedAscending(header.sortedColumnIndex);

            ColumnType column = (ColumnType)header.sortedColumnIndex;

            IOrderedEnumerable<AssetBuildInfo> assetOrdered = null;
            IOrderedEnumerable<BundleBuildInfo> bundleOrdered = null;

            switch (column)
            {
                case ColumnType.Name:
                case ColumnType.Object:
                    foreach (var bundleInfo in TreeViewData.Bundles)
                    {
                        assetOrdered = bundleInfo.Assets.Order(info => info.Name, ascending);
                        bundleInfo.Assets = new List<AssetBuildInfo>(assetOrdered);
                    }
                    bundleOrdered = TreeViewData.Bundles.Order(info => info.RelativePath, ascending);
                    break;

                case ColumnType.Group:
                    bundleOrdered = TreeViewData.Bundles.Order(info => info.Group, ascending);
                    break;

                case ColumnType.AssetCount:
                    bundleOrdered = TreeViewData.Bundles.Order(info => info.Assets.Count, ascending);
                    break;
                
                case ColumnType.Length:
                    foreach (var bundleInfo in TreeViewData.Bundles)
                    {
                        assetOrdered = bundleInfo.Assets.Order(info => info.Length, ascending);
                        bundleInfo.Assets = new List<AssetBuildInfo>(assetOrdered);
                    }
                    bundleOrdered = TreeViewData.Bundles.Order(info => info.AssetsLength, ascending);
                    break;


                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (bundleOrdered != null)
            {
                TreeViewData.Bundles = new List<BundleBuildInfo>(bundleOrdered);
            }

            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<BundleBuildInfo>()
            {
                id = 0, depth = -1, displayName = "Root", Data = null,
            };

            foreach (var bundleInfo in TreeViewData.Bundles)
            {
                var bundleNode = new TreeViewDataItem<BundleBuildInfo>()
                {
                    id = bundleInfo.RelativePath.GetHashCode(), displayName = $"{bundleInfo.RelativePath},{bundleInfo.Group}",Data = bundleInfo,
                };

                foreach (var assetInfo in bundleInfo.Assets)
                {
                    var assetNode = new TreeViewDataItem<AssetBuildInfo>()
                    {
                        id = assetInfo.Name.GetHashCode(), displayName = assetInfo.Name, Data = assetInfo,
                    };
                    bundleNode.AddChild(assetNode);
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
            var bundleItem = item as TreeViewDataItem<BundleBuildInfo>;
            var assetItem = item as TreeViewDataItem<AssetBuildInfo>;
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

               case ColumnType.AssetCount:
                   if (bundleItem != null)
                   {
                       EditorGUI.LabelField(cellRect,bundleItem.Data.Assets.Count.ToString(),centerStyle);
                   }
                   break;

                case ColumnType.Length:
                    ulong length = 0;
                    if (bundleItem != null)
                    {
                        length = bundleItem.Data.AssetsLength;
                    }
                    else
                    {
                        length = assetItem.Data.Length;
                    }
                    EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(length),centerStyle);
                    break;

                
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }
    }
}