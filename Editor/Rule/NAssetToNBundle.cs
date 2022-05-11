using System.Collections.Generic;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有资源分别构建为一个资源包
    /// </summary>
    public class NAssetToNBundle : IBundleBuildRule
    {
        public virtual List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory)
        {
            List<BundleBuildInfo> result = GetNAssetToNBundle(bundleBuildDirectory.DirectoryName,bundleBuildDirectory.Group,false);
            return result;
        }

        /// <summary>
        /// 将指定目录下所有资源分别构建为一个资源包
        /// </summary>
        protected List<BundleBuildInfo> GetNAssetToNBundle(string targetDirectory,string group, bool isRaw)
        {
            //注意：targetDirectory在这里被假设为一个形如Assets/xxx/yyy....格式的目录
            
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();
            
            if (Directory.Exists(targetDirectory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(targetDirectory);
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);//递归获取所有文件
             
                
                foreach (FileInfo file in files)
                {
                    if (Util.ExcludeSet.Contains(file.Extension))
                    {
                        continue;
                    }
                    
                    int firstIndex = targetDirectory.IndexOf("/");
                    int lastIndex = targetDirectory.LastIndexOf("/");
                    string directoryName;
                    string bundleName;
                    if (!isRaw)
                    {
                        directoryName = targetDirectory.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
                        bundleName = file.Name.Replace('.','_').ToLower() + ".bundle"; 
                    }
                    else
                    {
                        //原生资源包的bundleName和assetName要特殊处理下
                        directoryName = targetDirectory.Substring(firstIndex + 1);
                        bundleName = file.Name;  //以文件名作为原生资源包名
                    }
                    
               

                    BundleBuildInfo bundleBuildInfo =
                        new BundleBuildInfo(directoryName,bundleName, group, isRaw);

                    //获取Asset开头的资源全路径
                    int assetsIndex = file.FullName.IndexOf("Assets\\");
                    string assetName = file.FullName.Substring(assetsIndex).Replace('\\', '/');
                    bundleBuildInfo.Assets.Add(new AssetBuildInfo(assetName));
                    
                    result.Add(bundleBuildInfo);
                    
                }

               
            }
            
            return result;
        }
    }
}