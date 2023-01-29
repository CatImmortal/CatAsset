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
        /// 资源包清单信息列表
        /// </summary>
        public List<BundleManifestInfo> Bundles;

        /// <summary>
        /// 序列化前的预处理方法
        /// </summary>
        private void PreSerialize()
        {
            Bundles.Sort();
            foreach (BundleManifestInfo bundleManifestInfo in Bundles)
            {
                bundleManifestInfo.Assets.Sort();
            }
        }
        
        /// <summary>
        /// 反序列化的后处理方法
        /// </summary>
        private void PostDeserialize()
        {

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
            PreSerialize();
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

