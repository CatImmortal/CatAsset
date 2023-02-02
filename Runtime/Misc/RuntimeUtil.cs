
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 运行时工具类
    /// </summary>
    public static class RuntimeUtil
    {

        /// <summary>
        /// 内置Shader资源包名
        /// </summary>
        public const string BuiltInShaderBundleName = "UnityBuiltInShaders.bundle";
        
        private static StringBuilder CachedSB = new StringBuilder();
        
        public const int OneKB = 1024;
        public const int OneMB = OneKB * 1024;
        public const int OneGB = OneMB * 1024;
        

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
        public static string GetReadWritePath(string path,bool isUwrPath = false)
        {
            string result = GetRegularPath(Path.Combine(Application.persistentDataPath, path));

            if (isUwrPath && Application.platform == RuntimePlatform.Android)
            {
                //使用UnityWebRequest访问安卓下的Persistent路径 需要加file://头才行
                result = "file://" + result;
            }

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
        public static string GetByteLengthDesc(ulong length)
        {
            if (length > OneGB)
            {
                return (length / (OneGB * 1f)).ToString("0.00") + "G" ;
            }
            if (length > OneMB)
            {
                return (length / (OneMB * 1f)).ToString("0.00") + "M";
            }
            if (length > OneKB)
            {
                return (length / (OneKB * 1f)).ToString("0.00") + "K";
            }

            return length + "B";
        }

        /// <summary>
        /// 获取编辑器资源模式下的资源类别
        /// </summary>
        public static AssetCategory GetAssetCategoryInEditorMode(string assetName, Type assetType)
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
                   foreach (byte b in bytes)
                   {
                       CachedSB.Append(b.ToString("x2"));
                   }
                   string result = CachedSB.ToString();
                   CachedSB.Clear();
                   return result;
                }
            }

        }

        /// <summary>
        /// 校验读写区资源包文件
        /// </summary>
        public static bool VerifyReadWriteBundle(string path,BundleManifestInfo info,bool onlyLength = false)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            FileInfo fi = new FileInfo(path);
            bool isVerify = (ulong)fi.Length == info.Length;
            if (isVerify && !onlyLength)
            {
                //不是仅校验文件长度 就再校验MD5
                string md5 = GetFileMD5(path);
                isVerify = md5 == info.MD5;
            }

            return isVerify;
        }

        /// <summary>
        /// Web请求是否错误
        /// </summary>
        public static bool HasWebRequestError(UnityWebRequest uwr)
        {
#if UNITY_2020_3_OR_NEWER
            return uwr.result != UnityWebRequest.Result.Success;
#else
            return uwr.isNetworkError || uwr.isHttpError;
#endif
        }

       
    }
}
