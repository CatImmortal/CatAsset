using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 分析器信息界面基类
    /// </summary>
    public abstract class BaseProfilerInfoView
    {
        protected TreeViewState State;
        protected MultiColumnHeader Header;
        public BaseProfilerTreeView TreeView;

        /// <summary>
        /// 初始化TreeView
        /// </summary>
        public void InitTreeView()
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
            columns[0].minWidth = 400;
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

        /// <summary>
        /// 绘制信息界面
        /// </summary>
        public virtual void DrawInfoView(Rect position)
        {
            if (!TreeView.CanShow())
            {
                return;
            }


            TreeView.OnGUI(new Rect(0, 70, position.width, position.height - 70));
        }

    }
}
