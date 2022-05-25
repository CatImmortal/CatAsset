using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源管理器
    /// </summary>
    public static class CatAssetManager
    {
        /// <summary>
        /// 加载模式
        /// </summary>
        public static LoadMode LoadMode
        {
            get;
            set;
        }
        
        /// <summary>
        /// 是否开启编辑器资源模式
        /// </summary>
        public static bool IsEditorMode
        {
            get;
            set;
        }
        
        /// <summary>
        /// 轮询CatAsset管理器
        /// </summary>
        public static void Update()
        {
            
        }
        
        
    }
}

