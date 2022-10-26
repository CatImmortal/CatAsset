
using System;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Profiling;

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

        private static StringBuilder CachedSB = new StringBuilder();

        /// <summary>
        /// 获取规范的路径
        /// </summary>
        public static string GetRegularPath(string path)
        {
            return path.Replace('\\', '/');
        }


        /// <summary>
        /// 获取在只读区下的完整路径
        /// </summary>
        public static string GetReadOnlyPath(string path)
        {
            string result = GetRegularPath(Path.Combine(Application.streamingAssetsPath, path));
            return result;
        }


        /// <summary>
        /// 获取在读写区下的完整路径
        /// </summary>
        public static string GetReadWritePath(string path)
        {
            string result = GetRegularPath(Path.Combine(Application.persistentDataPath, path));
            return result;
        }

        /// <summary>
        /// 获取在远端下的完整路径
        /// </summary>
        public static string GetRemotePath(string path)
        {
            string result = GetRegularPath(Path.Combine(CatAssetUpdater.UpdateUriPrefix, path));
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

        /// <summary>
        /// 获取编辑器资源模式下的资源类别
        /// </summary>
        public static AssetCategory GetAssetCategoryWithEditorMode(string assetName, Type assetType)
        {
            if (assetName.StartsWith("Assets/"))
            {
                //资源名以Assets/开头
                if (typeof(UnityEngine.Object).IsAssignableFrom(assetType) || assetType == typeof(object))
                {
                    //以UnityEngine.Object及其派生类型或object为加载类型
                    //都视为内置资源包资源进行加载
                    return AssetCategory.InternalBundledAsset;
                }
                else
                {
                    //否则视为内置原生资源加载
                    return AssetCategory.InternalRawAsset;
                }
            }
            else
            {
                //资源名不以Assets/开头 视为外置原生资源加载
                return AssetCategory.ExternalRawAsset;
            }
        }

        /// <summary>
        /// 获取资源类别
        /// </summary>
        public static AssetCategory GetAssetCategory(string assetName)
        {
            if (!assetName.StartsWith("Assets/") && !assetName.StartsWith("Packages/"))
            {
                //资源名不以Assets/ 和 Packages/开头 是外置原生资源
                CatAssetDatabase.TryCreateExternalRawAssetRuntimeInfo(assetName);
                return AssetCategory.ExternalRawAsset;
            }

            AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetName);
            if (assetRuntimeInfo == null)
            {
                Debug.LogError($"GetAssetCategory调用失败，{assetName}的AssetRuntimeInfo为空，请检查资源名是否正确");
                return default;
            }

            if (assetRuntimeInfo.BundleManifest.IsRaw)
            {
                //内置原生资源
                return AssetCategory.InternalRawAsset;
            }

            //内置资源包资源
            return AssetCategory.InternalBundledAsset;
        }

        /// <summary>
        /// 获取文件MD5
        /// </summary>
        public static string GetFileMD5(string filePath)
        {
            using (FileStream fs = new FileStream(filePath,FileMode.Open))
            {
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                   byte[] bytes = md5.ComputeHash(fs);
                   CachedSB.Clear();
                   foreach (byte b in bytes)
                   {
                       CachedSB.Append(b.ToString("x2"));
                   }
                   string result = CachedSB.ToString();
                   return result;
                }
            }

        }
    }
}
