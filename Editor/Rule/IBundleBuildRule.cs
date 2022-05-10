using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建规则接口
    /// </summary>
    public interface IBundleBuildRule
    {
        /// <summary>
        /// 获取使用此规则构建的资源包构建信息列表
        /// </summary>
        List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory);
    }
}


