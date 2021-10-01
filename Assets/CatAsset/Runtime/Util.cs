using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System;

namespace CatAsset
{
    public static class Util
    {
        private static MD5 md5 = MD5.Create();
        
        /// <summary>
        /// 获取哈希值
        /// </summary>
        public static int GetHash(byte[] bytes)
        {
            byte[] md5Bytes = md5.ComputeHash(bytes);
            int hash = BitConverter.ToInt32(md5Bytes,0);
            return hash;
        }

        /// <summary>
        /// 获取资源清单文件名
        /// </summary>
        public static string GetManifestName()
        {
            string name = "CatAssetManifest.json";
            return name;
        }

        /// <summary>
        /// 获取在只读区下的完整路径
        /// </summary>
        public static string GetReadOnlyPath(string path)
        {
            string result = Path.Combine(Application.streamingAssetsPath, path);
            return result;
        }

        /// <summary>
        /// 获取在读写区下的完整路径
        /// </summary>
        public static string GetReadWritePath(string path)
        {
            string result = Path.Combine(Application.persistentDataPath, path);
            return result;
        }

        


    }

    

}
