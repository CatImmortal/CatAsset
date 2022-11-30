using CatAsset.Runtime;
using UnityEditor.IMGUI.Controls;

namespace CatAsset.Editor
{
    /// <summary>
    /// 分析器树视图基类
    /// </summary>
    public abstract class BaseProfilerTreeView : TreeView
    {
        /// <summary>
        /// 分析器信息
        /// </summary>
        public ProfilerInfo ProfilerInfo;

        protected BaseProfilerTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            useScrollView = true;
            showAlternatingRowBackgrounds = true;  //启用交替的行背景颜色，使每行的显示更清楚
            showBorder = true;  //在 TreeView 周围留出边距，以便显示一个细边框将其与其余内容分隔开
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }

        public virtual void OnSortingChanged(MultiColumnHeader header)
        {

        }

        public void Reload(ProfilerInfo info)
        {
            ProfilerInfo = info;
            Reload();
            OnSortingChanged(multiColumnHeader);
        }
    }
}
