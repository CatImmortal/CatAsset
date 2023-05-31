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
    /// 构建目录TreeView
    /// </summary>
    public class BuildDirectoryTreeView : BaseMultiColumnTreeView<BundleBuildConfigSO>
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
            /// 构建规则
            /// </summary>
            Rule,

            /// <summary>
            /// 过滤器
            /// </summary>
            Filter,
            
            /// <summary>
            /// 正则
            /// </summary>
            Regex,

            /// <summary>
            /// 资源组
            /// </summary>
            Group,
            
            /// <summary>
            /// 压缩设置
            /// </summary>
            CompressOption,
            
            /// <summary>
            /// 加密设置
            /// </summary>
            EncryptOption,
            
            /// <summary>
            /// 删除按钮
            /// </summary>
            RemoveButton,
        }

        /// <summary>
        /// 目录的规则名
        /// </summary>
        private string[] directoryRuleNames;

        /// <summary>
        /// 文件的规则名
        /// </summary>
        private string[] fileRuleNames;


        public BuildDirectoryTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state,
            multiColumnHeader)
        {
            Reload(BundleBuildConfigSO.Instance);
        }

        public override bool CanShow()
        {
            return TreeViewData != null && TreeViewData.Directories.Count > 0;
        }

        protected override void OnSortingChanged(MultiColumnHeader header)
        {
            if (header.sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = header.IsSortedAscending(header.sortedColumnIndex);

            ColumnType column = (ColumnType)header.sortedColumnIndex;

            IOrderedEnumerable<BundleBuildDirectory> ordered = null;

            switch (column)
            {
                case ColumnType.Name:
                case ColumnType.Object:
                    ordered = TreeViewData.Directories.Order(info => info.DirectoryName, ascending);
                    break;

                case ColumnType.Rule:
                    ordered = TreeViewData.Directories.Order(info => info.BuildRuleName, ascending);
                    break;

                case ColumnType.Filter:
                    ordered = TreeViewData.Directories.Order(info => info.Filter, ascending);
                    break;
                
                case ColumnType.Regex:
                    ordered = TreeViewData.Directories.Order(info => info.Regex, ascending);
                    break;

                case ColumnType.Group:
                    ordered = TreeViewData.Directories.Order(info => info.Group, ascending);
                    break;
                
                case ColumnType.CompressOption:
                    ordered = TreeViewData.Directories.Order(info => info.CompressOption.ToString(), ascending);
                    break;
                
                case ColumnType.RemoveButton:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (ordered != null)
            {
                TreeViewData.Directories = new List<BundleBuildDirectory>(ordered);
                Reload();
            }
        }


        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewDataItem<BundleBuildDirectory>()
            {
                id = 0, depth = -1, displayName = "Root", Data = null,
            };

            foreach (var bundleBuildDirectory in TreeViewData.Directories)
            {
                var node = new TreeViewDataItem<BundleBuildDirectory>()
                {
                    id = bundleBuildDirectory.DirectoryName.GetHashCode(),
                    displayName = bundleBuildDirectory.DirectoryName, Data = bundleBuildDirectory,
                };

                root.AddChild(node);
            }

            SetupDepthsFromParentsAndChildren(root);

            return root;
        }


        protected override void RowGUI(RowGUIArgs args)
        {
            EditorGUI.BeginChangeCheck();
            
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), args.item, (ColumnType)args.GetColumn(i), ref args);
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(TreeViewData);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// 绘制指定行每一列的内容
        /// </summary>
        private void CellGUI(Rect cellRect, TreeViewItem item, ColumnType column, ref RowGUIArgs args)
        {
           
            
            var directoryItem = (TreeViewDataItem<BundleBuildDirectory>)item;
            GUIStyle centerStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
            centerStyle.normal = new GUIStyleState() { textColor = Color.white };
            switch (column)
            {
                case ColumnType.Name:
                    args.rowRect = cellRect;
                    args.label = directoryItem.Data.DirectoryName;
                    base.RowGUI(args);
                    break;

                case ColumnType.Object:
                    EditorGUI.BeginDisabledGroup(true);
                    if (directoryItem.Data.DirectoryObj == null && !string.IsNullOrEmpty(directoryItem.Data.DirectoryName))
                    {
                        directoryItem.Data.DirectoryObj =
                            AssetDatabase.LoadAssetAtPath<Object>(directoryItem.Data.DirectoryName);
                    }
                    EditorGUI.ObjectField(cellRect, directoryItem.Data.DirectoryObj, typeof(Object), false);
                    EditorGUI.EndDisabledGroup();
                    break;

                case ColumnType.Rule:
                    bool isFile = !AssetDatabase.IsValidFolder(directoryItem.Data.DirectoryName);
                    string[] ruleNames = GetRuleNames(isFile);
                    int index = 0;
                    for (int i = 0; i < ruleNames.Length; i++)
                    {
                        if (ruleNames[i] == directoryItem.Data.BuildRuleName)
                        {
                            index = i;
                        }
                    }

                    index = EditorGUI.Popup(cellRect, index, ruleNames);
                    directoryItem.Data.BuildRuleName = ruleNames[index];
                    break;

                case ColumnType.Filter:
                    directoryItem.Data.Filter = EditorGUI.TextField(cellRect, directoryItem.Data.Filter);
                    break;
                
                case ColumnType.Regex:
                    directoryItem.Data.Regex = EditorGUI.TextField(cellRect, directoryItem.Data.Regex);
                    break;

                case ColumnType.Group:
                    directoryItem.Data.Group = EditorGUI.TextField(cellRect, directoryItem.Data.Group);
                    break;
                
                case ColumnType.CompressOption:
                    directoryItem.Data.CompressOption =
                        (BundleCompressOptions)EditorGUI.EnumPopup(cellRect, directoryItem.Data.CompressOption);
                    break;
                
                case ColumnType.EncryptOption:
                    directoryItem.Data.EncryptOption =
                        (BundleEncryptOptions)EditorGUI.EnumPopup(cellRect, directoryItem.Data.EncryptOption);
                    break;
                
                case ColumnType.RemoveButton:
                    var oldColor = GUI.color;
                    GUI.color = Color.red;
                    if (GUI.Button(cellRect,"X"))
                    {
                        TreeViewData.Directories.Remove(directoryItem.Data);
                        if (CanShow())
                        {
                            Reload();
                        }
                    }
                    GUI.color = oldColor;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }

        /// <summary>
        /// 获取构建规则名列表
        /// </summary>
        /// <returns></returns>
        private string[] GetRuleNames(bool isFile)
        {
            string[] GetRuleNames()
            {
                List<string> list = new List<string>();
                foreach (var pair in TreeViewData.GetRuleDict())
                {
                    IBundleBuildRule rule = pair.Value;
                    if (isFile == rule.IsFile)
                    {
                        list.Add(rule.GetType().Name);
                    }
                }

                return list.ToArray();
            }

            if (!isFile)
            {
                if (fileRuleNames == null)
                {
                    fileRuleNames = GetRuleNames();
                }

                return fileRuleNames;
            }
            else
            {
                if (directoryRuleNames == null)
                {
                    directoryRuleNames = GetRuleNames();
                }

                return directoryRuleNames;
            }



        }
    }
}