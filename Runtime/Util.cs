using System.IO;
using UnityEngine;

namespace CatAsset.Runtime
{
    public class Util
    {
        /// <summary>
        /// 资源清单文件名
        /// </summary>
        public const string ManifestFileName = "CatAssetManifest.json";

        
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