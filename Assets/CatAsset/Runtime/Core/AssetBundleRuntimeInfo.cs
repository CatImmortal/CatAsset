using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// AssetBundle运行时信息
    /// </summary>
    public class AssetBundleRuntimeInfo
    {
        /// <summary>
        /// AssetBundle清单信息
        /// </summary>
        public AssetBundleManifestInfo ManifestInfo;

        /// <summary>
        /// 已加载的AssetBundle实例
        /// </summary>
        public AssetBundle AssetBundle;

        /// <summary>
        /// 是否位于读写区
        /// </summary>
        public bool InReadWrite;

        private string loadPath;

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
                        loadPath = Util.GetReadWritePath(ManifestInfo.AssetBundleName);
                    }
                    else
                    {
                        loadPath = Util.GetReadOnlyPath(ManifestInfo.AssetBundleName);
                    }
                }
                return loadPath;
            }
        }

        /// <summary>
        /// 是否加载失败
        /// </summary>
        public bool IsLoadFailed;

        private HashSet<string> usedAsset;

        /// <summary>
        /// 当前使用中的Asset
        /// </summary>
        public HashSet<string> UsedAsset
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
    }
}

