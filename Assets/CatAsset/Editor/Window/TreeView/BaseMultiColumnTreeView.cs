using UnityEditor.IMGUI.Controls;

namespace CatAsset.Editor
{
    /// <summary>
    /// 多列TreeView基类
    /// </summary>
    public abstract class BaseMultiColumnTreeView<T> : TreeView ,IMultiColumnTreeView
    {
        /// <summary>
        /// TreeView数据
        /// </summary>
        protected T TreeViewData;
        
        /// <inheritdoc/>
        public string SearchString
        {
            get => searchString;
            set => searchString = value;
        }
        
        protected BaseMultiColumnTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            useScrollView = true;
            showAlternatingRowBackgrounds = true;  //启用交替的行背景颜色，使每行的显示更清楚
            showBorder = true;  //在 TreeView 周围留出边距，以便显示一个细边框将其与其余内容分隔开
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }


        protected virtual void OnSortingChanged(MultiColumnHeader header)
        {

        }


        private void Reload(T treeViewData)
        {
            TreeViewData = treeViewData;

            if (!CanShow())
            {
                return;
            }
            
            Reload();
            OnSortingChanged(multiColumnHeader);
        }


        /// <inheritdoc/>
        public void Reload(object treeViewData)
        {
            Reload((T)treeViewData);
        }

        /// <summary>
        /// 是否可显示
        /// </summary>
        public virtual bool CanShow()
        {
            return true;
        }
    }
}