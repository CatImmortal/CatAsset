using System;
using System.Collections.Generic;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源缓存清单
    /// </summary>
    [Serializable]
    public class AssetCacheManifest
    {
        [Serializable]
        public class AssetCache
        {
            public string Name;
            public string MD5;
        }
        
        /// <summary>
        /// 资源清单Json文件名
        /// </summary>
        public const string ManifestJsonFileName = "AssetCacheManifest.json";


        public List<AssetCache> Caches = new List<AssetCache>();

        public Dictionary<string, string> GetCacheDict()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (AssetCache assetCache in Caches)
            {
                result.Add(assetCache.Name,assetCache.MD5);
            }

            return result;
        }
    }
}