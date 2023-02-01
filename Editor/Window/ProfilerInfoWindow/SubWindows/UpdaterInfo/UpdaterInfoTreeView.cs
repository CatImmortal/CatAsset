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
    /// 更新器信息TreeView
    /// </summary>
    public class UpdaterInfoTreeView : BaseMultiColumnTreeView<ProfilerInfo>
    {
        /// <summary>
        /// 列类型
        /// </summary>
        private enum ColumnType
        {

            Name,
            State,
            
            WaitingCount,
            WaitingLength,
            
            UpdatingCount,
            UpdatingLength,
            
            UpdatedCount,
            UpdatedLength,
            
            TotalCount,
            TotalLength,
            
            DownloadBytesLength,
            Speed,
            Progress,
            

           
        }

        public UpdaterInfoTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {

        }

        public override bool CanShow()
        {
            return TreeViewData != null && TreeViewData.UpdaterInfoList.Count > 0;
        }

        protected override void OnSortingChanged(MultiColumnHeader header)
        {
            if (header.sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = header.IsSortedAscending(header.sortedColumnIndex);

            ColumnType column = (ColumnType)header.sortedColumnIndex;

            IOrderedEnumerable<ProfilerUpdaterInfo> updaterOrdered = null;
            IOrderedEnumerable<ProfilerUpdaterInfo.BundleInfo> bundleOrdered = null;
            switch (column)
            {
                case ColumnType.Name:
                    foreach (var updaterInfo in TreeViewData.UpdaterInfoList)
                    {
                        bundleOrdered = updaterInfo.UpdateBundleInfos.Order(info => info.Name, ascending);
                        updaterInfo.UpdateBundleInfos = new List<ProfilerUpdaterInfo.BundleInfo>(bundleOrdered);
                    }
                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.Name, ascending);
                    break;
                
                case ColumnType.State:
                    foreach (var updaterInfo in TreeViewData.UpdaterInfoList)
                    {
                        bundleOrdered = updaterInfo.UpdateBundleInfos.Order(info => info.State, ascending);
                        updaterInfo.UpdateBundleInfos = new List<ProfilerUpdaterInfo.BundleInfo>(bundleOrdered);
                    }
                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.State, ascending);
                    break;

                case ColumnType.WaitingCount:
                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.WaitingCount, ascending);
                    break;
                
                case ColumnType.WaitingLength:
                    foreach (var updaterInfo in TreeViewData.UpdaterInfoList)
                    {
                        bundleOrdered = updaterInfo.UpdateBundleInfos.Order(info => info.Length, ascending);
                        updaterInfo.UpdateBundleInfos = new List<ProfilerUpdaterInfo.BundleInfo>(bundleOrdered);
                    }

                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.WaitingLength, ascending);
                    break;

                case ColumnType.UpdatingCount:
                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.UpdatingCount, ascending);
                    break;
                
                case ColumnType.UpdatingLength:
                    foreach (var updaterInfo in TreeViewData.UpdaterInfoList)
                    {
                        bundleOrdered = updaterInfo.UpdateBundleInfos.Order(info => info.Length, ascending);
                        updaterInfo.UpdateBundleInfos = new List<ProfilerUpdaterInfo.BundleInfo>(bundleOrdered);
                    }

                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.UpdatingLength, ascending);
                    break;

                case ColumnType.UpdatedCount:
                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.UpdatedCount, ascending);
                    break;
                
                case ColumnType.UpdatedLength:
                    foreach (var updaterInfo in TreeViewData.UpdaterInfoList)
                    {
                        bundleOrdered = updaterInfo.UpdateBundleInfos.Order(info => info.Length, ascending);
                        updaterInfo.UpdateBundleInfos = new List<ProfilerUpdaterInfo.BundleInfo>(bundleOrdered);
                    }

                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.UpdatedLength, ascending);
                    break;
                

                case ColumnType.TotalCount:
                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.TotalCount, ascending);
                    break;
                
                case ColumnType.TotalLength:
                    foreach (var updaterInfo in TreeViewData.UpdaterInfoList)
                    {
                        bundleOrdered = updaterInfo.UpdateBundleInfos.Order(info => info.Length, ascending);
                        updaterInfo.UpdateBundleInfos = new List<ProfilerUpdaterInfo.BundleInfo>(bundleOrdered);
                    }

                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.TotalLength, ascending);
                    break;
                
                case ColumnType.DownloadBytesLength:
                    foreach (var updaterInfo in TreeViewData.UpdaterInfoList)
                    {
                        bundleOrdered = updaterInfo.UpdateBundleInfos.Order(info => info.DownLoadedBytesLength, ascending);
                        updaterInfo.UpdateBundleInfos = new List<ProfilerUpdaterInfo.BundleInfo>(bundleOrdered);
                    }

                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.DownloadedBytesLength, ascending);
                    break;
                
                case ColumnType.Speed:
                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.Speed, ascending);
                    break;
                
                case ColumnType.Progress:
                    foreach (var updaterInfo in TreeViewData.UpdaterInfoList)
                    {
                        bundleOrdered = updaterInfo.UpdateBundleInfos.Order(info => info.Progress, ascending);
                        updaterInfo.UpdateBundleInfos = new List<ProfilerUpdaterInfo.BundleInfo>(bundleOrdered);
                    }

                    updaterOrdered = TreeViewData.UpdaterInfoList.Order(info => info.Progress, ascending);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (updaterOrdered != null)
            {
                TreeViewData.UpdaterInfoList = new List<ProfilerUpdaterInfo>(updaterOrdered);
                Reload();
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<ProfilerUpdaterInfo>()
            {
                id = 0, depth = -1, displayName = "Root",Data = null,
            };

            foreach (var updaterInfo in TreeViewData.UpdaterInfoList)
            {
                var updaterNode = new TreeViewDataItem<ProfilerUpdaterInfo>()
                {
                    id = updaterInfo.Name.GetHashCode(), displayName = updaterInfo.Name, Data = updaterInfo,
                };

                foreach (var bundleInfo in updaterInfo.UpdateBundleInfos)
                {
                    var bundleNode = new TreeViewDataItem<ProfilerUpdaterInfo.BundleInfo>()
                    {
                        id = bundleInfo.Name.GetHashCode(), displayName = updaterInfo.Name, Data = bundleInfo,
                    };
                    
                    updaterNode.AddChild(bundleNode);
                }

                root.AddChild(updaterNode);
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
            var updaterItem = item as TreeViewDataItem<ProfilerUpdaterInfo>;
            var bundleItem = item as TreeViewDataItem<ProfilerUpdaterInfo.BundleInfo>;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};

            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    if (updaterItem != null)
                    {
                        args.label = updaterItem.Data.Name;
                    }
                    else
                    {
                        args.label = bundleItem.Data.Name;
                    }
                    base.RowGUI(args);
                    break;
                
                case ColumnType.State:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,updaterItem.Data.State.ToString(),centerStyle);
                    }
                    else
                    {
                        EditorGUI.LabelField(cellRect,bundleItem.Data.State.ToString(),centerStyle);
                    }
                    break;
                
               
                
                case ColumnType.WaitingCount:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,updaterItem.Data.WaitingCount.ToString(),centerStyle);
                    }
                    else
                    {
                        if (bundleItem.Data.State == UpdateState.Waiting)
                        {
                            EditorGUI.LabelField(cellRect,"1",centerStyle);
                        }
                    }
                    break;
                
                case ColumnType.WaitingLength:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(updaterItem.Data.WaitingLength),centerStyle);
                    }
                    else 
                    {
                        if (bundleItem.Data.State == UpdateState.Waiting)
                        {
                            EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(bundleItem.Data.Length),centerStyle);
                        }
                    }
                    break;

                case ColumnType.UpdatingCount:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,updaterItem.Data.UpdatingCount.ToString(),centerStyle);
                    }
                    else
                    {
                        if (bundleItem.Data.State == UpdateState.Updating)
                        {
                            EditorGUI.LabelField(cellRect,"1",centerStyle);
                        }
                    }
                    break;
                
                case ColumnType.UpdatingLength:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(updaterItem.Data.UpdatingLength),centerStyle);
                    }
                    else 
                    {
                        if (bundleItem.Data.State == UpdateState.Updating)
                        {
                            EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(bundleItem.Data.Length),centerStyle);
                        }
                    }
                    break;

                
                case ColumnType.UpdatedCount:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,updaterItem.Data.UpdatedCount.ToString(),centerStyle);
                    }
                    else
                    {
                        if (bundleItem.Data.State == UpdateState.Updated)
                        {
                            EditorGUI.LabelField(cellRect,"1",centerStyle);
                        }
                    }
                    break;
                
                case ColumnType.UpdatedLength:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(updaterItem.Data.UpdatedLength),centerStyle);
                    }
                    else 
                    {
                        if (bundleItem.Data.State == UpdateState.Updated)
                        {
                            EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(bundleItem.Data.Length),centerStyle);
                        }
                    }
                    break;
                

                
                case ColumnType.TotalCount:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,updaterItem.Data.UpdateBundleInfos.Count.ToString(),centerStyle);
                    }
                    break;
                
                case ColumnType.TotalLength:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(updaterItem.Data.TotalLength),centerStyle);
                    }
                    else 
                    {
                        EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(bundleItem.Data.Length),centerStyle);
                    }
                    break;
                
                case ColumnType.DownloadBytesLength:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(updaterItem.Data.DownloadedBytesLength),centerStyle);
                    }
                    else 
                    {
                        EditorGUI.LabelField(cellRect,RuntimeUtil.GetByteLengthDesc(bundleItem.Data.DownLoadedBytesLength),centerStyle);
                    }
                    break;
                
                case ColumnType.Speed:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,$"{RuntimeUtil.GetByteLengthDesc(updaterItem.Data.Speed)}/s",centerStyle);
                    }
                    break;
                
                case ColumnType.Progress:
                    if (updaterItem != null)
                    {
                        EditorGUI.LabelField(cellRect,$"{updaterItem.Data.Progress * 100:0.00}%",centerStyle);
                    }
                    else 
                    {
                        EditorGUI.LabelField(cellRect,$"{bundleItem.Data.Progress * 100:0.00}%",centerStyle);
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
       
        }
    }
}
