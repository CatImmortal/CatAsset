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

                    //
                    // int suffixIndex = assetName.LastIndexOf('.');
                    //
                    // //资源包名是不带后缀的资源名
                    // string bundleName = Util.GetBundleName(assetName.Remove(suffixIndex));
                    
                    int firstIndex = bundleBuildDirectory.DirectoryName.IndexOf("/");
                    int lastIndex = bundleBuildDirectory.DirectoryName.LastIndexOf("/");
                    string directoryName = bundleBuildDirectory.DirectoryName.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
                    string bundleName = file.Name.Replace('.','_').ToLower() + ".bundle";  //以文件名作为资源包名

                    BundleBuildInfo bundleBuildInfo =
                        new BundleBuildInfo(directoryName,bundleName, bundleBuildDirectory.Group, false);

                    //获取Asset开头的资源全路径
                    int assetsIndex = file.FullName.IndexOf("Assets\\");
                    string assetName = file.FullName.Substring(assetsIndex).Replace('\\', '/');
                    bundleBuildInfo.Assets.Add(new AssetBuildInfo(assetName));
                    
                    result.Add(bundleBuildInfo);
                    
                }

                
            }

            return result;
        }

        // protected List<BundleBuildInfo> GetNAssetToNBundle(string directory, bool isRaw)
        // {
        //     
        // }
    }
}