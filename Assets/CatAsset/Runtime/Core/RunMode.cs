using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 运行模式
    /// </summary>
    public enum RunMode
    {
        /// <summary>
        /// 仅使用安装包内资源（单机模式）
        /// </summary>
        PackageOnly,

        /// <summary>
        /// 可更新模式
        /// </summary>
        Updatable,
        
        /// <summary>
        /// 加载时若本地不存在会下载的可更新模式（边玩边下）
        /// </summary>
        UpdatableWhilePlaying,
    }
}

