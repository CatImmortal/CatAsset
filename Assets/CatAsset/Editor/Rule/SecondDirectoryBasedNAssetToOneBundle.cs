using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将指定目录下所有二级子目录各自使用NAssetToOneBundle规则进行构建
    /// </summary>
    public class SecondDirectoryBasedNAssetToOneBundle : NAssetToOneBundle
    {
        /// <inheritdoc />
        public override List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory,
            HashSet<string> lookedAssets)
        {
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();

            if (Directory.Exists(bundleBuildDirectory.DirectoryName))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(bundleBuildDirectory.DirectoryName);

                //获取所有一级目录
                DirectoryInfo[] topDirectories = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < topDirectories.Length; i++)
                {
                    
                    //获取所有二级目录
                    DirectoryInfo topDirInfo = topDirectories[i];
                    DirectoryInfo[] secondDirectories = topDirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

                    for (int j = 0; j < secondDirectories.Length; j++)
                    {
                         //每个二级目录构建成一个资源包
                         DirectoryInfo secondDirInfo = secondDirectories[j];
                         EditorUtility.DisplayProgressBar($"{nameof(SecondDirectoryBasedNAssetToOneBundle)}", $"{secondDirInfo.FullName}", j / (secondDirectories.Length * 1.0f));
                         string assetsDir = EditorUtil.FullNameToAssetName(secondDirInfo.FullName);
                         BundleBuildInfo info = GetNAssetToOneBundle(assetsDir, bundleBuildDirectory, lookedAssets);
                         result.Add(info);
                    }
                    
                }
            }

            return result;
        }
    }
}