using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace CatAsset.Editor
{
    /// <summary>
    /// 打包规则
    /// </summary>
    [Serializable]
    public class PackageRule
    {
        public string Directory;
        public PackageMode Mode;

    }
}

