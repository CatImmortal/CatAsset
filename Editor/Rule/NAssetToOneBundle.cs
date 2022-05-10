using System.Collections.Generic;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有资源构建为一个资源包
    /// </summary>
    public class NAssetToOneBundle : IBundleBuildRule
    {
        public List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory)
        {
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();

            if (Directory.Exists(bundleBuildDirectory.DirectoryName))
            {
                List<string> assetNames = new List<string>();
                
                DirectoryInfo dirInfo = new DirectoryInfo(bundleBuildDirectory.DirectoryName);
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);  //递归获取所有文件
                
                foreach (FileInfo file in files)
                {
                    if (Util.ExcludeSet.Contains(file.Extension))
                    {
                        //跳过不该打包的文件
                        continue;
                    }
                    
                    int index = file.FullName.IndexOf("Assets\\");
                    string path = file.FullName.Substring(index); //获取Asset开头的资源全路径
                    assetNames.Add(path.Replace('\\', '/'));
                }

                if (assetNames.Count == 0)
                {
                    //空包不打
                    return null;
                }

               
                
                string bundleName = Util.GetBundleName(bundleBuildDirectory.DirectoryName);
                string group = bundleBuildDirectory.Group;
                BundleBuildInfo bundleBuildInfo = new BundleBuildInfo(bundleName,group,false);
                for (int i = 0; i < assetNames.Count; i++)
                {
                    AssetBuildInfo assetBuildInfo = new AssetBuildInfo(assetNames[i]);
                    bundleBuildInfo.Assets.Add(assetBuildInfo);
                }

                //此构建规则只返回一个资源包
                result.Add(bundleBuildInfo);

            }

            return result;
        }
    }
}