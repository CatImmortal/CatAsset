using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

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
        protected List<BundleBuildInfo> GetNAssetToNBundle(string targetDirectory,string ruleRegex,string group, bool isRaw)
        {
            //注意：targetDirectory在这里被假设为一个形如Assets/xxx/yyy....格式的目录
            
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();
           
            if (Directory.Exists(targetDirectory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(targetDirectory);
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);//递归获取所有文件
             
                
                foreach (FileInfo file in files)
                {
                    string assetName = Util.FullNameToAssetName(file.FullName);//Assets/xxx/yyy.zz
                    
                    if (!Util.IsValidAsset(assetName))
                    {
                        continue;
                    }
                    
                    if (!string.IsNullOrEmpty(ruleRegex) && !Regex.IsMatch(assetName,ruleRegex))
                    {
                        continue;
                    }
                    
                    string assetsDir = Util.FullNameToAssetName(file.Directory.FullName); //Assets/xxx
                    string directoryName = assetsDir.Substring(assetsDir.IndexOf("/") + 1); //去掉Assets/
                    string bundleName;
                    if (!isRaw)
                    { 
                        bundleName = file.Name.Replace('.','_') + ".bundle"; 
                    }
                    else
                    {
                        //直接以文件名作为原生资源包名
                        bundleName = file.Name;
                    }
                    
                    BundleBuildInfo bundleBuildInfo =
                        new BundleBuildInfo(directoryName,bundleName, group, isRaw);
                    
                    
                     bundleBuildInfo.Assets.Add(new AssetBuildInfo(assetName));
                    
                    result.Add(bundleBuildInfo);
                    
                }

               
            }
            
            return result;
        }
    }
}