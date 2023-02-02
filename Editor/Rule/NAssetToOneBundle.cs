using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有资源构建为一个资源包
    /// </summary>
    public class NAssetToOneBundle : IBundleBuildRule
    {
        /// <inheritdoc />
        public virtual bool IsRaw => false;
        
        /// <inheritdoc />
        public bool IsFile => false;
        
        /// <inheritdoc />
        public virtual List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory,
            HashSet<string> lookedAssets)
        {
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();

            if (Directory.Exists(bundleBuildDirectory.DirectoryName))
            {
                //此构建规则只返回一个资源包
                BundleBuildInfo info = GetNAssetToOneBundle(bundleBuildDirectory.DirectoryName, bundleBuildDirectory, lookedAssets);
                result.Add(info);
            }

            return result;
        }

        /// <summary>
        /// 将指定目录下所有资源构建为一个资源包
        /// </summary>
        protected BundleBuildInfo GetNAssetToOneBundle(string buildDirectory,BundleBuildDirectory bundleBuildDirectory,HashSet<string> lookedAssets)
        {
            //注意：buildDirectory在这里被假设为一个形如Assets/xxx/yyy....格式的目录
          
            string[] guids = AssetDatabase.FindAssets(bundleBuildDirectory.Filter, new[] { buildDirectory });
            List<string> assetNames = new List<string>(guids.Length);
            
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                if (lookedAssets.Contains(path))
                {
                    //被其他构建规则处理过了 跳过
                    continue;
                }
                lookedAssets.Add(path);
                
                if (!EditorUtil.IsValidAsset(path))
                {
                    //不是有效的资源文件 跳过
                    continue;
                }
                if (!string.IsNullOrEmpty(bundleBuildDirectory.Regex) && !Regex.IsMatch(path,bundleBuildDirectory.Regex))
                {
                    //不匹配正则 跳过
                    continue;
                }
                assetNames.Add(path);
            }
            
            
            //Assets/xxx/yyy
            int firstIndex = buildDirectory.IndexOf("/");
            int lastIndex = buildDirectory.LastIndexOf("/");
            string directoryName;
            if (firstIndex != lastIndex)
            {
                //Assets/xxx/yyy -> xxx
                directoryName = buildDirectory.Substring(firstIndex + 1, lastIndex - firstIndex - 1);  
            }
            else
            {
                //Assets/xxx -> xxx
                directoryName = buildDirectory.Substring(firstIndex + 1);
            }
          
            string bundleName = buildDirectory.Substring(lastIndex + 1) + ".bundle"; //以构建目录名作为资源包名 yyy.bundle

            BundleBuildInfo bundleBuildInfo = new BundleBuildInfo(directoryName, bundleName, bundleBuildDirectory.Group,
                false, bundleBuildDirectory.GetCompressOption(), bundleBuildDirectory.GetEncryptOption());
            bundleBuildInfo.Assets.Capacity = assetNames.Count;
            
            for (int i = 0; i < assetNames.Count; i++)
            {
                AssetBuildInfo assetBuildInfo = new AssetBuildInfo(assetNames[i]);
                bundleBuildInfo.Assets.Add(assetBuildInfo);
            }

            return bundleBuildInfo;
        }
    }
}