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

        /// <summary>
        /// 获取资源清单文件名
        /// </summary>
        public static string GetManifestFileName()
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
