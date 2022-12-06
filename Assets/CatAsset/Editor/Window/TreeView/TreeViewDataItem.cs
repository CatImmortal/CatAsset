using UnityEditor.IMGUI.Controls;

namespace CatAsset.Editor
{
    /// <summary>
    /// TreeView数据项
    /// </summary>
    public class TreeViewDataItem<T> : TreeViewItem
    {
        /// <summary>
        /// 数据
        /// </summary>
        public T Data;
    }
}
