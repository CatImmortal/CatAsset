using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包信息树视图
    /// </summary>
    public class BundleInfoTreeView : TreeView
    {
        private int idGenerator = 0;

        public ProfilerInfo ProfilerInfo;

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

        public BundleInfoTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state,multiColumnHeader)
        {
            useScrollView = true;
            showAlternatingRowBackgrounds = true;  //启用交替的行背景颜色，使每行的显示更清楚
            showBorder = true;  //在 TreeView 周围留出边距，以便显示一个细边框将其与其余内容分隔开
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }

        private void OnSortingChanged(MultiColumnHeader header)
        {

        }

        protected override TreeViewItem BuildRoot()
        {
            idGenerator = 0;

            var root = new TreeViewDataItem<ProfilerBundleInfo>()
            {
                id = idGenerator++, depth = -1, displayName = "Root",Data = null,
            };

            foreach (var bundleInfo in ProfilerInfo.BundleInfoList)
            {
                var bundleNode = new TreeViewDataItem<ProfilerBundleInfo>()
                {
                    id = idGenerator++, displayName = bundleInfo.RelativePath,Data = bundleInfo,
                };
                root.AddChild(bundleNode);

                foreach (var assetInfo in bundleInfo.InMemoryAssets)
                {
                    var assetNode = new TreeViewDataItem<ProfilerAssetInfo>()
                    {
                        id = idGenerator++, displayName = assetInfo.Name, Data = assetInfo,
                    };
                    bundleNode.AddChild(assetNode);
                }
            }

            SetupDepthsFromParentsAndChildren(root);

            return root;
        }

        // protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        // {
        //     List<TreeViewItem> result = new List<TreeViewItem>();
        //     TreeViewDataItem<ProfilerBundleInfo> item = (TreeViewDataItem<ProfilerBundleInfo>)root;
        //     if (item.Data == null)
        //     {
        //         foreach (var bundleInfo in profilerInfo.BundleInfoList)
        //         {
        //             result.Add(new TreeViewDataItem<ProfilerBundleInfo>(idGenerator++,0,bundleInfo.RelativePath,bundleInfo));
        //         }
        //
        //         return result;
        //     }
        //     else
        //     {
        //
        //     }
        //     return result;
        // }

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
            //CenterRectUsingSingleLineHeight(ref cellRect);
            TreeViewDataItem<ProfilerBundleInfo> bundleItem = item as TreeViewDataItem<ProfilerBundleInfo>;
            TreeViewDataItem<ProfilerAssetInfo> assetItem = item as TreeViewDataItem<ProfilerAssetInfo>;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};
            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
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

                case ColumnType.RefCount:
                    if (assetItem != null)
                    {
                        EditorGUI.LabelField(cellRect,assetItem.Data.RefCount.ToString(),centerStyle);
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
