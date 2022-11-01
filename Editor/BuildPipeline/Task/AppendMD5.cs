using System.IO;
using CatAsset.Runtime;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 附加MD5到资源包名中
    /// </summary>
    public class AppendMD5 : IBuildTask
    {
        [InjectContext(ContextUsage.In)] 
        private IManifestParam manifestParam;
        
        [InjectContext(ContextUsage.In)] 
        private IBundleBuildParameters buildParam;
        
        public int Version { get; }
        
        public ReturnCode Run()
        {
            CatAssetManifest manifest = manifestParam.Manifest;
            string outputFolder = ((BundleBuildParameters) buildParam).OutputFolder;

            foreach (BundleManifestInfo bundleManifestInfo in manifest.Bundles)
            {
                string oldPath = Path.Combine(outputFolder,  bundleManifestInfo.RelativePath);
                
                //获取附加了MD5的资源包名
                string[] nameArray = bundleManifestInfo.BundleName.Split('.');
                string md5BundleName =   $"{nameArray[0]}_{bundleManifestInfo.MD5}.{nameArray[1]}";  
                bundleManifestInfo.BundleName = md5BundleName;

                //获取新的相对路径
                string newRelativePath = RuntimeUtil.GetRegularPath(Path.Combine(bundleManifestInfo.Directory,
                    bundleManifestInfo.BundleName)); 
                    
                
                string newPath = Path.Combine(outputFolder,  newRelativePath);
                
                File.Move(oldPath,newPath);
            }
            
            return ReturnCode.Success;
        }

        
    }
}