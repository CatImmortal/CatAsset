using System.Collections.Generic;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有资源分别构建为一个资源包
    /// </summary>
    public class NAssetToNBundle : IBundleBuildRule
    {
        public List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory)
        {
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();
            
            if (Directory.Exists(bundleBuildDirectory.DirectoryName))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(bundleBuildDirectory.DirectoryName);
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);//递归获取所有文件
             
                
                foreach (FileInfo file in files)
                {
                    if (Util.ExcludeSet.Contains(file.Extension))
                    {
                        //跳过不该打包的文件
                        continue;
                    }
                 

                    int assetsIndex = file.FullName.IndexOf("Assets\\");
                    string assetName = file.FullName.Substring(assetsIndex).Replace('\\', '/');

                    int suffixIndex = assetName.LastIndexOf('.');

                    //资源包名是不带后缀的资源名
                    string bundleName = Util.GetBundleName(assetName.Remove(suffixIndex));

                    BundleBuildInfo bundleBuildInfo =
                        new BundleBuildInfo(bundleName, bundleBuildDirectory.Group, false);

                    bundleBuildInfo.Assets.Add(new AssetBuildInfo(assetName));
                    
                    result.Add(bundleBuildInfo);
                    
                }

                
            }

            return result;
        }
    }
}