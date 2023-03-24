using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 多列TreeView子窗口基类
    /// </summary>
    public abstract class BaseTreeViewSubWindow : BaseSubWindow
    {
        protected TreeViewState State;
        
        protected MultiColumnHeader Header;
        
        public IMultiColumnTreeView TreeView { get; protected set; }

        /// <inheritdoc/>
        public override void InitSubWindow()
        {
            InitTreeView();
        }
        
        /// <inheritdoc/>
        public override void DrawSubWindow(Rect position)
        {
            if (!TreeView.CanShow())
            {
                return;
            }

            TreeView.OnGUI(new Rect(0, 70, position.width, position.height - 70));
        }

        /// <summary>
        /// 初始化TreeView
        /// </summary>
        private void InitTreeView()
        {
            List<string> columnList = GetColumns();

            MultiColumnHeaderState.Column[] columns = CreateColumns(columnList);
            ProcessColumns(columns);

            var state = new MultiColumnHeaderState(columns);

            Header = new MultiColumnHeader(state);
            Header.ResizeToFit();

            State = new TreeViewState();
            CreateTreeView();
        }

        /// <summary>
        /// 获取行数据
        /// </summary>
        protected abstract List<string> GetColumns();

        /// <summary>
        /// 处理行数据
        /// </summary>
        protected virtual void ProcessColumns(MultiColumnHeaderState.Column[] columns)
        {
            columns[0].minWidth = 400;  //名称
        }
        
        /// <summary>
        /// 创建列的数组
        /// </summary>
        private MultiColumnHeaderState.Column[] CreateColumns(List<string> columnList)
        {
            var columns = new MultiColumnHeaderState.Column[columnList.Count];
            for (int i = 0; i < columns.Length; i++)
            {
                string name = columnList[i];
                columns[i] = new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent(name),
                    headerTextAlignment = TextAlignment.Center,
                    sortingArrowAlignment = TextAlignment.Right
                };
            }

            return columns;
        }

        /// <summary>
        /// 创建TreeView
        /// </summary>
        protected abstract void CreateTreeView();
        
     
    }
}