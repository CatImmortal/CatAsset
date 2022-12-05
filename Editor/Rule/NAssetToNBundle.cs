using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有资源分别构建为一个资源包
    /// </summary>
    public class NAssetToNBundle : IBundleBuildRule
    {
        /// <inheritdoc />
        public virtual bool IsRaw => false;
        
        /// <inheritdoc />
        public virtual List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory)
        {
            List<BundleBuildInfo> result = GetNAssetToNBundle(bundleBuildDirectory.DirectoryName,bundleBuildDirectory.RuleRegex,bundleBuildDirectory.Group,false);
            return result;
        }

        /// <summary>
        /// 将指定目录下所有资源分别构建为一个资源包
        /// </summary>
        protected List<BundleBuildInfo> GetNAssetToNBundle(string buildDirectory,string ruleRegex,string group, bool isRaw)
        {
            //注意：buildDirectory在这里被假设为一个形如Assets/xxx/yyy....格式的目录
            
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();

            if (!Directory.Exists(buildDirectory))
            {
                return result;
            }

            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { buildDirectory });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!EditorUtil.IsValidAsset(path))
                {
                    //不是有效的资源文件 跳过
                    continue;
                }

                if (!string.IsNullOrEmpty(ruleRegex) && !Regex.IsMatch(path,ruleRegex))
                {
                    //不匹配正则 跳过
                    continue;
                }

                FileInfo fi = new FileInfo(path);
                string assetDir = EditorUtil.FullNameToAssetName(fi.Directory.FullName);//由Assets/开头的目录
                string directoryName = assetDir.Substring(assetDir.IndexOf('/') + 1);//去掉Assets/的目录
                string bundleName;
              
                if (!isRaw)
                { 
                    bundleName = fi.Name.Replace('.','_') + ".bundle"; 
                }
                else
                {
                    //直接以文件名作为原生资源包名
                    bundleName = fi.Name;
                }
                
                BundleBuildInfo bundleBuildInfo =
                    new BundleBuildInfo(directoryName,bundleName, group, isRaw);

                bundleBuildInfo.Assets.Add(new AssetBuildInfo(path));
                    
                result.Add(bundleBuildInfo);
                
            }

            return result;
        }
    }
}