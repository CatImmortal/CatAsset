using System.Collections.Generic;
using System.IO;
using CatJson;
using UnityEngine;
namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源更新器
    /// </summary>
    public static class CatAssetUpdater
    {
        /// <summary>
        /// 资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/BundleRelativePath 为下载地址
        /// </summary>
        public static string UpdateUriPrefix;
        
        /// <summary>
        /// 资源包相对路径->读写区资源包清单信息，用于生成读写区资源清单
        /// </summary>
        private static Dictionary<string, BundleManifestInfo> readWriteManifestInfoDict = new Dictionary<string, BundleManifestInfo>();

        /// <summary>
        /// 添加读写区资源包清单信息
        /// </summary>
        internal static void AddReadWriteManifestInfo(BundleManifestInfo info)
        {
            readWriteManifestInfoDict[info.RelativePath] = info;
        }
        
        /// <summary>
        /// 移除读写区资源包清单信息
        /// </summary>
        internal static void RemoveReadWriteManifestInfo(BundleManifestInfo info)
        {
            readWriteManifestInfoDict.Remove(info.RelativePath);
        }
        
        /// <summary>
        /// 清空读写区资源包清单信息
        /// </summary>
        internal static void ClearReadWriteManifestInfo()
        {
            readWriteManifestInfoDict.Clear();
        }
        
        /// <summary>
        /// 生成读写区资源清单
        /// </summary>
        internal static void GenerateReadWriteManifest()
        {
            //创建 CatAssetManifest
            CatAssetManifest manifest = new CatAssetManifest();
            manifest.GameVersion = Application.version;
            List<BundleManifestInfo> bundleInfos = new List<BundleManifestInfo>(readWriteManifestInfoDict.Count);
            foreach (KeyValuePair<string, BundleManifestInfo> item in readWriteManifestInfoDict)
            {
                bundleInfos.Add(item.Value);
            }
            bundleInfos.Sort();
            manifest.Bundles = bundleInfos;

            //写入清单文件json
            string json = JsonParser.ToJson(manifest);
            string path = Util.GetReadWritePath(Util.ManifestFileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(json);
            }
        }
        
        
    }
}