using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器信息
    /// </summary>
    [Serializable]
    public class ProfilerInfo
    {
        public ProfilerInfoType Type;
        public List<ProfilerBundleInfo> BundleInfo;

        /// <summary>
        /// 序列化
        /// </summary>
        public static byte[] Serialize(ProfilerInfo profilerInfo)
        {
            using MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms,profilerInfo);
            var bytes = ms.GetBuffer();
            Debug.Log($"分析器信息序列化数据大小：{bytes.Length}");
            return bytes;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static ProfilerInfo Deserialize(byte[] bytes)
        {
            using MemoryStream ms = new MemoryStream(bytes);
            ms.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            var profilerInfo = (ProfilerInfo)bf.Deserialize(ms);
            return profilerInfo;
        }
    }
}
