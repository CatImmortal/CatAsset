using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 多列TreeView接口
    /// </summary>
    public interface IMultiColumnTreeView
    {
        /// <summary>
        /// 搜索字符串
        /// </summary>
        string SearchString { get; set; }
        
        /// <summary>
        /// 重载
        /// </summary>
        void Reload(object treeViewData);

        /// <summary>
        /// 是否可显示
        /// </summary>
        bool CanShow();

        /// <summary>
        /// 绘制
        /// </summary>
        void OnGUI(Rect rect);
        
        /// <summary>
        /// 全部展开
        /// </summary>
        void ExpandAll();

        /// <summary>
        /// 全部收起
        /// </summary>
        void CollapseAll();
    }
}