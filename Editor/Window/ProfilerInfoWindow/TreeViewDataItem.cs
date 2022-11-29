using UnityEditor.IMGUI.Controls;

namespace CatAsset.Editor
{
    /// <summary>
    /// 树视图数据项
    /// </summary>
    public class TreeViewDataItem<T> : TreeViewItem
    {
        public T Data;
    }
}
