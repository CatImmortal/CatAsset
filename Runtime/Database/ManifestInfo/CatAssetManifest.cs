using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源清单
    /// </summary>
    [Serializable]
    public class CatAssetManifest
    {
        /// <summary>
        /// 资源清单Json文件名
        /// </summary>
        public const string ManifestJsonFileName = "CatAssetManifest.json";


        /// <summary>
        /// 游戏版本号
        /// </summary>
        public string GameVersion;

        /// <summary>
        /// 清单版本号
        /// </summary>
        public int ManifestVersion;

        /// <summary>
        /// 目标平台
        /// </summary>
        public string Platform;

        /// <summary>
        /// 是否附加MD5值到资源包名中
        /// </summary>
        public bool IsAppendMD5;

        /// <summary>
        /// 资源包清单信息列表
        /// </summary>
        public List<BundleManifestInfo> Bundles;

        /// <summary>
        /// 资源清单信息列表
        /// </summary>
        public List<AssetManifestInfo> Assets;
        
        /// <summary>
        /// 反序列化的后处理方法
        /// </summary>
        private void PostDeserialize()
        {
            //从ID还原引用
            foreach (BundleManifestInfo bundleManifestInfo in Bundles)
            {
                bundleManifestInfo.IsAppendMD5 = IsAppendMD5;
                bundleManifestInfo.Assets = new List<AssetManifestInfo>(bundleManifestInfo.AssetIDs.Count);
                foreach (int assetID in bundleManifestInfo.AssetIDs)
                {
                    bundleManifestInfo.Assets.Add(Assets[assetID]);
                }
            }

            foreach (AssetManifestInfo assetManifestInfo in Assets)
            {
                assetManifestInfo.Dependencies = new List<AssetManifestInfo>(assetManifestInfo.DependencyIDs.Count);
                foreach (int dependencyID in assetManifestInfo.DependencyIDs)
                {
                    assetManifestInfo.Dependencies.Add(Assets[dependencyID]);
                }
            }
        }

        /// <summary>
        /// 序列化为二进制数据
        /// </summary>
        public byte[] SerializeToBinary()
        {
            return null;
        }

        /// <summary>
        /// 从二进制数据反序列化
        /// </summary>
        public static CatAssetManifest DeserializeFromBinary(byte[] bytes)
        {
            return null;
        }

        /// <summary>
        /// 序列化为Json文本
        /// </summary>
        public string SerializeToJson()
        {
            string json = JsonUtility.ToJson(this,true);
            return json;
        }

        /// <summary>
        /// 从Json文本反序列化
        /// </summary>
        public static CatAssetManifest DeserializeFromJson(string json)
        {
            CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(json);
            manifest.PostDeserialize();
            return manifest;
        }
    }
}

