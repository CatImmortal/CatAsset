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
        private string loadPath;
        private HashSet<string> usedAsset;

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
    }
}

