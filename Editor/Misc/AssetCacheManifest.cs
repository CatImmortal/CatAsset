using System;
using System.Collections.Generic;
using System.IO;
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
            public long LastWriteTime;
            public long MetaLastWriteTime;
            
            public static AssetCacheInfo Create(string assetName)
            {
                AssetCacheInfo assetCacheInfo = new AssetCacheInfo
                {
                    Name = assetName,
                    LastWriteTime = File.GetLastWriteTime(assetName).Ticks,
                    MetaLastWriteTime =  File.GetLastWriteTime($"{assetName}.meta").Ticks,
                };
                return assetCacheInfo;
            }

            public static bool operator ==(AssetCacheInfo a,AssetCacheInfo b)
            {
                return Equals(a, b);
            }
            
            public static bool operator !=(AssetCacheInfo a,AssetCacheInfo b)
            {
                return !(a == b);
            }
            
            public bool Equals(AssetCacheInfo other)
            {
                return Name == other.Name && LastWriteTime == other.LastWriteTime && MetaLastWriteTime == other.MetaLastWriteTime;
            }

            public override bool Equals(object obj)
            {
                return obj is AssetCacheInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Name != null ? Name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ LastWriteTime.GetHashCode();
                    hashCode = (hashCode * 397) ^ MetaLastWriteTime.GetHashCode();
                    return hashCode;
                }
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