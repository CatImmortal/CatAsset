using System.Collections.Generic;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 将单个资源构建为单个资源包
    /// </summary>
    public class OneAssetToOnBundle : IBundleBuildRule
    {
        /// <inheritdoc />
        public virtual bool IsRaw => false;
        
        /// <inheritdoc />
        public bool IsFile => true;
        
        /// <inheritdoc />
        public List<BundleBuildInfo> GetBundleList(BundleBuildDirectory bundleBuildDirectory, HashSet<string> lookedAssets)
        {
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();
            
            string path = bundleBuildDirectory.DirectoryName;
            if (!EditorUtil.IsValidAsset(path))
            {
                //不是有效的资源文件 跳过
                return result;
            }

            BundleBuildInfo info = GetOneAssetToOnBundle(bundleBuildDirectory,lookedAssets);
            result.Add(info);
            
            return result;
        }

        /// <summary>
        /// 将单个资源构建为单个资源包
        /// </summary>
        private BundleBuildInfo GetOneAssetToOnBundle(BundleBuildDirectory bundleBuildDirectory,HashSet<string> lookedAssets)
        {
            string path = bundleBuildDirectory.DirectoryName;
            
            lookedAssets.Add(path);

            FileInfo fi = new FileInfo(path);
            string assetDir = EditorUtil.FullNameToAssetName(fi.Directory.FullName);//由Assets/开头的目录
            string directoryName = assetDir.Substring(assetDir.IndexOf('/') + 1);//去掉Assets/的目录
            string bundleName = fi.Name.Replace('.','_') + ".bundle"; 
            
            BundleBuildInfo bundleBuildInfo =
                new BundleBuildInfo(directoryName,bundleName, bundleBuildDirectory.Group, IsRaw,bundleBuildDirectory.GetCompressOption());

            bundleBuildInfo.Assets.Add(new AssetBuildInfo(path));
            

            return bundleBuildInfo;
        }
    }
}