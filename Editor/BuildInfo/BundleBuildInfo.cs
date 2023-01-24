using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatAsset.Runtime;
using UnityEditor;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建信息
    /// </summary>
    [Serializable]
    public class BundleBuildInfo : IComparable<BundleBuildInfo>,IEquatable<BundleBuildInfo>
    {
        /// <summary>
        /// 目录名
        /// </summary>
        public string DirectoryName;
        
        /// <summary>
        /// 资源包名
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 唯一资源包名
        /// </summary>
        public string UniqueBundleName;
        
        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

        /// <summary>
        /// 是否为原生资源包
        /// </summary>
        public bool IsRaw;
        
        /// <summary>
        /// 总资源长度
        /// </summary>
        public ulong AssetsLength;
        
        /// <summary>
        /// 资源包压缩设置
        /// </summary>
        public BundleCompressOptions CompressOption;
        
        /// <summary>
        /// 资源构建信息列表
        /// </summary>
        public List<AssetBuildInfo> Assets = new List<AssetBuildInfo>();

        public BundleBuildInfo(string directoryName, string bundleName,string group,bool isRaw,BundleCompressOptions compressOption)
        {
            DirectoryName = directoryName;
            BundleName = bundleName;
            Group = group;
            IsRaw = isRaw;
            CompressOption = compressOption;
            UniqueBundleName = RuntimeUtil.GetRegularPath(Path.Combine(DirectoryName, BundleName));
        }

        /// <summary>
        /// 刷新总资源长度
        /// </summary>
        public void RefreshAssetsLength()
        {
            foreach (AssetBuildInfo assetBuildInfo in Assets)
            {
                AssetsLength += assetBuildInfo.Length;
            }
        }

        /// <summary>
        /// 获取用于构建资源包的AssetBundleBuild
        /// </summary>
        public AssetBundleBuild GetAssetBundleBuild()
        {
            AssetBundleBuild bundleBuild = new AssetBundleBuild
            {
                assetBundleName = UniqueBundleName
            };

            List<string> assetNames = new List<string>();
            foreach (AssetBuildInfo assetBuildInfo in Assets)
            {
                assetNames.Add(assetBuildInfo.Name);
            }

            bundleBuild.assetNames = assetNames.ToArray();

            return bundleBuild;
        }
        
        public override string ToString()
        {
            return UniqueBundleName;
        }
        
        public int CompareTo(BundleBuildInfo other)
        {
            return UniqueBundleName.CompareTo(other.UniqueBundleName);
        }
        
        public bool Equals(BundleBuildInfo other)
        {
            return UniqueBundleName.Equals(other.UniqueBundleName);
        }

        public override int GetHashCode()
        {
            return UniqueBundleName.GetHashCode();
        }
        
        
    }

}
