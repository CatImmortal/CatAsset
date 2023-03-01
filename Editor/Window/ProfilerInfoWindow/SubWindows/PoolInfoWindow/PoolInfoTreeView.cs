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
    /// 对象池信息TreeView
    /// </summary>
    public class PoolInfoTreeView : BaseMultiColumnTreeView<ProfilerInfo>
    {
        /// <summary>
        /// 列类型
        /// </summary>
        private enum ColumnType
        {
            Name,
            
            PoolExpireTime,
            ObjExpireTime,
            UnusedTimer,
            
            AllCount,
            UsedCount,
            UnusedCount,
            
            IsLock,
        }
        
        public PoolInfoTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            
        }

        public override bool CanShow()
        {
            return TreeViewData != null && TreeViewData.PoolInfoList.Count > 0;
        }
        
          protected override void OnSortingChanged(MultiColumnHeader header)
        {
            if (header.sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = header.IsSortedAscending(header.sortedColumnIndex);

            ColumnType column = (ColumnType)header.sortedColumnIndex;

            IOrderedEnumerable<ProfilerPoolInfo> poolOrdered = null;

            switch (column)
            {
                case ColumnType.Name:
                    poolOrdered = TreeViewData.PoolInfoList.Order(info => info.Name, ascending);
                    break;
                
                case ColumnType.PoolExpireTime:
                    poolOrdered = TreeViewData.PoolInfoList.Order(info => info.PoolExpireTime, ascending);
                    break;
                
                case ColumnType.ObjExpireTime:
                    poolOrdered = TreeViewData.PoolInfoList.Order(info => info.ObjExpireTime, ascending);
                    break;
                
                case ColumnType.UnusedTimer:
                    foreach (var poolInfo in TreeViewData.PoolInfoList)
                    {
                        var objOrdered = poolInfo.PoolObjectList.Order((info) =>  info.UnusedTimer , ascending);
                        poolInfo.PoolObjectList = new List<ProfilerPoolInfo.PoolObjectInfo>(objOrdered);
                    }
                    poolOrdered = TreeViewData.PoolInfoList.Order(info => info.UnusedTimer, ascending);
                    break;
                
                case ColumnType.AllCount:
                    poolOrdered = TreeViewData.PoolInfoList.Order(info => info.AllCount, ascending);
                    break;
                
                case ColumnType.UsedCount:
                    poolOrdered = TreeViewData.PoolInfoList.Order(info => info.UnusedCount, ascending);
                    break;
                
                case ColumnType.UnusedCount:
                    poolOrdered = TreeViewData.PoolInfoList.Order(info => info.UnusedCount, ascending);
                    break;
                
                case ColumnType.IsLock:
                    foreach (var poolInfo in TreeViewData.PoolInfoList)
                    {
                        var objOrdered = poolInfo.PoolObjectList.Order((info) =>  info.IsLock , ascending);
                        poolInfo.PoolObjectList = new List<ProfilerPoolInfo.PoolObjectInfo>(objOrdered);
                    }
                    Reload();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (poolOrdered != null)
            {
                TreeViewData.PoolInfoList = new List<ProfilerPoolInfo>(poolOrdered);
                Reload();
            }
        }
        
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<ProfilerPoolInfo>()
            {
                id = 0, depth = -1, displayName = "Root",Data = null,
            };

            foreach (var poolInfo in TreeViewData.PoolInfoList)
            {
                var poolNode = new TreeViewDataItem<ProfilerPoolInfo>()
                {
                    id = poolInfo.Name.GetHashCode(), displayName = $"{poolInfo.Name}",Data = poolInfo,
                };

                foreach (var objInfo in poolInfo.PoolObjectList)
                {
                    var objNode = new TreeViewDataItem<ProfilerPoolInfo.PoolObjectInfo>()
                    {
                        id = objInfo.InstanceID, displayName = objInfo.InstanceID.ToString(), Data = objInfo,
                    };

                    poolNode.AddChild(objNode);
                }

              
                root.AddChild(poolNode);
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
            var poolItem = item as TreeViewDataItem<ProfilerPoolInfo>;
            var objItem = item as TreeViewDataItem<ProfilerPoolInfo.PoolObjectInfo>;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState(){textColor = Color.white};
            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    if (poolItem != null)
                    {
                        args.label = poolItem.Data.Name;
                    }
                    else
                    {
                        args.label = objItem.Data.InstanceID.ToString();
                    }
                    base.RowGUI(args);
                    break;
                
                case ColumnType.PoolExpireTime:
                    if (poolItem != null)
                    {
                        EditorGUI.LabelField(cellRect, poolItem.Data.PoolExpireTime.ToString(),centerStyle);
                    }
                    break;
                
                case ColumnType.ObjExpireTime:
                    if (poolItem != null)
                    {
                        EditorGUI.LabelField(cellRect, poolItem.Data.ObjExpireTime.ToString(),centerStyle);
                    }
                    break;
                
                case ColumnType.UnusedTimer:
                    if (poolItem != null)
                    {
                        EditorGUI.LabelField(cellRect, $"{(poolItem.Data.UnusedTimer):0.00}",centerStyle);
                    }
                    else
                    {
                        EditorGUI.LabelField(cellRect, $"{(objItem.Data.UnusedTimer):0.00}",centerStyle);
                    }
                    break;

                case ColumnType.AllCount:
                    if (poolItem != null)
                    {
                        EditorGUI.LabelField(cellRect,poolItem.Data.AllCount.ToString(),centerStyle);
                    }
                    break;
                
                case ColumnType.UsedCount:
                    if (poolItem != null)
                    {
                        EditorGUI.LabelField(cellRect,poolItem.Data.UsedCount.ToString(),centerStyle);
                    }

                    if (objItem != null && objItem.Data.Used)
                    {
                        EditorGUI.LabelField(cellRect,"1",centerStyle);
                    }
                    break;
                
                case ColumnType.UnusedCount:
                    if (poolItem != null)
                    {
                        EditorGUI.LabelField(cellRect,poolItem.Data.UnusedCount.ToString(),centerStyle);
                    }
                    
                    if (objItem != null && !objItem.Data.Used)
                    {
                        EditorGUI.LabelField(cellRect,"1",centerStyle);
                    }
                    break;
                
                case ColumnType.IsLock:
                    if (objItem != null)
                    {
                        EditorGUI.LabelField(cellRect, objItem.Data.IsLock.ToString(),centerStyle);
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }
    }
}