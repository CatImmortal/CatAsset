using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Free,

        /// <summary>
        /// 等待其他任务
        /// </summary>
        WaitOther,

        /// <summary>
        /// 执行中
        /// </summary>
        Executing,

        /// <summary>
        /// 执行完毕
        /// </summary>
        Done,
    }

}

