using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// Bundle运行时信息
    /// </summary>
    public class BundleRuntimeInfo
    {
        private string loadPath;
        private HashSet<string> usedAsset;
        private HashSet<string> dependencyBundles;

        /// <summary>
        /// Bundle清单信息
        /// </summary>
        public BundleManifestInfo ManifestInfo;

        /// <summary>
        /// Bundle实例
        /// </summary>
        public AssetBundle Bundle;

        /// <summary>
        /// 是否位于读写区
        /// </summary>
        public bool InReadWrite;

        /// <summary>
        /// 依赖计数（表示有多少个Bundle依赖此Bundle，每个依赖Bundle只计数1次）
        /// </summary>
        public int DependencyCount;

        /// <summary>
        /// 加载地址
        /// </summary>
        public string LoadPath
        {
            get
            {
                if (loadPath == null)
                {
                    if (InReadWrite)
                    {
                        loadPath = Util.GetReadWritePath(ManifestInfo.BundleName);
                    }
                    else
                    {
                        loadPath = Util.GetReadOnlyPath(ManifestInfo.BundleName);
                    }
                }
                return loadPath;
            }
        }

        /// <summary>
        /// 当前使用中的Asset，这里面的Asset的UseCount都大于0
        /// </summary>
        public HashSet<string> UsedAssets
        {
            get
            {
                if (usedAsset == null)
                {
                    usedAsset = new HashSet<string>();
                }

                return usedAsset;
            }
        }

        /// <summary>
        /// 此Bundle所依赖的Bundle（在卸载Bundle时要把依赖的Bundle的依赖计数-1）
        /// </summary>
        public HashSet<string> DependencyBundles
        {
            get
            {
                if (dependencyBundles == null)
                {
                    dependencyBundles = new HashSet<string>();
                }
                return dependencyBundles;
            }
        }
    }
}

