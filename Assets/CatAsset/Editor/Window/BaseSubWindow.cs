using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 子窗口基类
    /// </summary>
    public abstract class BaseSubWindow
    {
        /// <summary>
        /// 初始化子窗口
        /// </summary>
        public abstract void InitSubWindow();
        
        /// <summary>
        /// 绘制子窗口
        /// </summary>
        public abstract void DrawSubWindow(Rect position);
    }
}