using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 版本检查结果
    /// </summary>
    public enum CheckVersionResult
    {
        /// <summary>
        /// 不需要更新
        /// </summary>
        None,

        /// <summary>
        /// 需要更新整包
        /// </summary>
        UpdateGame,

        /// <summary>
        /// 需要更新资源包
        /// </summary>
        UpdateAsset,
    }

}
