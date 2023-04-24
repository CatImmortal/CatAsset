using System;
using System.Collections.Generic;
using CatAsset.Runtime;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源缓存清单
    /// </summary>
    [Serializable]
    public class AssetCacheManifest
    {
        /// <summary>
        /// 资源缓存信息
        /// </summary>
        [Serializable]
        public struct AssetCacheInfo : IEquatable<AssetCacheInfo>
        {
            public string Name;
            public string MD5;
            public string MetaMD5;
            
            public bool Equals(AssetCacheInfo other)
            {
                return Name == other.Name && MD5 == other.MD5 && MetaMD5 == other.MetaMD5;
            }

            public override bool Equals(object obj)
            {
                return obj is AssetCacheInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, MD5, MetaMD5);
            }
            
            public static bool operator ==(AssetCacheInfo a,AssetCacheInfo b)
            {
                return Equals(a, b);
            }
            
            public static bool operator !=(AssetCacheInfo a,AssetCacheInfo b)
            {
                return !(a == b);
            }

            public static AssetCacheInfo Create(string assetName)
            {
                AssetCacheInfo assetCacheInfo = new AssetCacheInfo
                {
                    Name = assetName,
                    MD5 = RuntimeUtil.GetFileMD5(assetName),
                    MetaMD5 = RuntimeUtil.GetFileMD5($"{assetName}.meta")
                };
                return assetCacheInfo;
            }
          
        }
        
        /// <summary>
        /// 资源清单Json文件名
        /// </summary>
        public const string ManifestJsonFileName = "AssetCacheManifest.json";


        public List<AssetCacheInfo> Caches = new List<AssetCacheInfo>();

        public Dictionary<string, AssetCacheInfo> GetCacheDict()
        {
            Dictionary<string, AssetCacheInfo> result = new Dictionary<string, AssetCacheInfo>();
            foreach (AssetCacheInfo assetCache in Caches)
            {
                result.Add(assetCache.Name,assetCache);
            }

            return result;
        }
        
        
    }
}