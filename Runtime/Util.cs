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

        private const int oneKB = 1024;
        private const int oneMB = oneKB * 1024;
        private const int oneGB = oneMB * 1024;
        
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
        
        /// <summary>
        /// 根据字节长度获取合适的描述信息
        /// </summary>
        public static string GetByteLengthDesc(long length)
        {
            if (length > oneGB)
            {
                return (length / (oneGB * 1f)).ToString("0.00") + "G" ;
            }
            if (length > oneMB)
            {
                return (length / (oneMB * 1f)).ToString("0.00") + "M";
            }
            if (length > oneKB)
            {
                return (length / (oneKB * 1f)).ToString("0.00") + "K";
            }

            return length + "B";
        }
    }
}