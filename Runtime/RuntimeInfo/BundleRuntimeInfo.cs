using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包运行时信息
    /// </summary>
    public class BundleRuntimeInfo
    {
        private string loadPath;
        private HashSet<string> usedAsset;

        /// <summary>
        /// 资源包清单信息
        /// </summary>
        public BundleManifestInfo Manifest;

        /// <summary>
        /// 资源包实例
        /// </summary>
        public AssetBundle Bundle;

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
                        loadPath = Util.GetReadWritePath(Manifest.RelativePath);
                    }
                    else
                    {
                        loadPath = Util.GetReadOnlyPath(Manifest.RelativePath);
                    }
                }
                return loadPath;
            }
        }

        /// <summary>
        /// 当前使用中的资源集合，这里面的资源的引用计数都大于0
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

        public override string ToString()
        {
            return Manifest.ToString();
        }
    }
}

