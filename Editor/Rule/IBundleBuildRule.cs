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
        /// 此规则所构建的是否为原生资源包
        /// </summary>
        /// <returns></returns>
        bool IsRaw { get; }
        
        /// <summary>
        /// 此规则所处理的目录是否为一个文件
        /// </summary>
        bool IsFile { get; }
        
        /// <summary>
        /// 获取使用此规则构建的资源包构建信息列表
        /// </summary>
        List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory, HashSet<string> lookedAssets);
    }
}


