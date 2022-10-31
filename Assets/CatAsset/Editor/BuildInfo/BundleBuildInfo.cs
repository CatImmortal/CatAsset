using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// 相对路径
        /// </summary>
        public string RelativePath;
        
        /// <summary>
        /// 目录名
        /// </summary>
        public string DirectoryName;
        
        /// <summary>
        /// 资源包名
        /// </summary>
        public string BundleName;

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
        public long AssetsLength;
        
        /// <summary>
        /// 资源构建信息列表
        /// </summary>
        public List<AssetBuildInfo> Assets = new List<AssetBuildInfo>();

        public BundleBuildInfo(string directoryName, string bundleName,string group,bool isRaw)
        {
            DirectoryName = directoryName;
            BundleName = bundleName;
            Group = group;
            IsRaw = isRaw;
            RelativePath = Runtime.Util.GetRegularPath(Path.Combine(DirectoryName, BundleName));
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
                assetBundleName = RelativePath
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
            return RelativePath;
        }
        
        public int CompareTo(BundleBuildInfo other)
        {
            return RelativePath.CompareTo(other.RelativePath);
        }
        
        public bool Equals(BundleBuildInfo other)
        {
            return RelativePath.Equals(other.RelativePath);
        }

        public override int GetHashCode()
        {
            return RelativePath.GetHashCode();
        }
        
        
    }

}
