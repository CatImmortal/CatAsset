using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        /// 资源清单二进制文件名
        /// </summary>
        public const string ManifestBinaryFileName = "CatAssetManifest.data";

        /// <summary>
        /// 序列化版本
        /// </summary>
        public const int SerializeVersion = 0;
        
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
        /// 序列化二进制数据前的预处理方法
        /// </summary>
        private void PreSerializeToBinary()
        {
            Bundles.Sort();
            foreach (BundleManifestInfo bundleManifestInfo in Bundles)
            {
                bundleManifestInfo.Assets.Sort();
            }
        }
        
        /// <summary>
        /// 反序列化二进制数据的后处理方法
        /// </summary>
        private void PostDeserializeFromBinary()
        {
        }

        /// <summary>
        /// 序列化为二进制数据
        /// </summary>
        public byte[] SerializeToBinary()
        {
            PreSerializeToBinary();

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms,Encoding.UTF8))
                {
                    writer.Write(SerializeVersion);
                    writer.Write(GameVersion);
                    writer.Write(ManifestVersion);
                    writer.Write(Platform);

                    writer.Write(Bundles.Count);
                    foreach (BundleManifestInfo bundleManifestInfo in Bundles)
                    {
                        bundleManifestInfo.Serialize(writer);
                    }

                    byte[] bytes = ms.ToArray();
            
                    return bytes;
                }
            }

        }

        /// <summary>
        /// 从二进制数据反序列化
        /// </summary>
        public static CatAssetManifest DeserializeFromBinary(byte[] bytes)
        {

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms,Encoding.UTF8))
                {
                    CatAssetManifest manifest = new CatAssetManifest();
            
                    int serializeVersion = reader.ReadInt32();
                    manifest.GameVersion = reader.ReadString();
                    manifest.ManifestVersion = reader.ReadInt32();
                    manifest.Platform = reader.ReadString();

                    int count = reader.ReadInt32();
                    manifest.Bundles = new List<BundleManifestInfo>(count);
                    for (int i = 0; i < count; i++)
                    {
                        BundleManifestInfo bundleManifestInfo = BundleManifestInfo.Deserialize(reader,serializeVersion);
                        manifest.Bundles.Add(bundleManifestInfo);
                    }

                    manifest.PostDeserializeFromBinary();
            
                    return manifest;
                }
            }
            
           
        }

        /// <summary>
        /// 序列化为Json文本
        /// </summary>
        public string SerializeToJson()
        {
            Bundles.Sort();
            foreach (BundleManifestInfo bundleManifestInfo in Bundles)
            {
                bundleManifestInfo.Assets.Sort();
            }
            
            string json = JsonUtility.ToJson(this,true);
            return json;
        }

        /// <summary>
        /// 从Json文本反序列化
        /// </summary>
        public static CatAssetManifest DeserializeFromJson(string json)
        {
            CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(json);
            return manifest;
        }

        /// <summary>
        /// 将清单写入文件
        /// </summary>
        public void WriteFile(string directory, bool isBinary)
        {
            if (!isBinary)
            {
                //写入清单文件json
                string path = RuntimeUtil.GetRegularPath(Path.Combine(directory, ManifestJsonFileName));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                string json = SerializeToJson();
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.Write(json);
                }
            }
            else
            {
                //写入清单文件binary
                string path = RuntimeUtil.GetRegularPath(Path.Combine(directory, ManifestBinaryFileName));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                byte[] bytes = SerializeToBinary();
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    fs.Write(bytes,0,bytes.Length);
                }
            }
        }
    }
}

