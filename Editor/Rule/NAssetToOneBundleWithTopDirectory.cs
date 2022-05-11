using System.Collections.Generic;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有一级子目录各自使用NAssetToOneBundle规则进行构建
    /// </summary>
    public class NAssetToOneBundleWithTopDirectory : NAssetToOneBundle
    {
        public override List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory)
        {
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();

            if (Directory.Exists(bundleBuildDirectory.DirectoryName))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(bundleBuildDirectory.DirectoryName);

                //获取所有一级目录
                DirectoryInfo[] topDirectories = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

                foreach (DirectoryInfo topDirInfo in topDirectories)
                {
                    //每个一级目录构建成一个资源包
                    int assetsIndex = topDirInfo.FullName.IndexOf("Assets\\");
                    string directory = topDirInfo.FullName.Substring(assetsIndex).Replace('\\', '/');
                    BundleBuildInfo info = GetNAssetToOneBundle(directory, bundleBuildDirectory.Group);
                    result.Add(info);
                }
            }

            return result;
        }
    }
}