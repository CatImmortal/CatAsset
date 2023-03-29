using System.IO;
using CatAsset.Runtime;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 写入清单文件到平台缓存文件夹中
    /// </summary>
    public class WriteManifestFileToCache : IBuildTask
    {
        [InjectContext(ContextUsage.In)]
        private IBundleBuildConfigParam configParam;
        
        [InjectContext(ContextUsage.InOut)]
        private IManifestParam manifestParam;
        
        public int Version { get; }
        
        public ReturnCode Run()
        {
            string folder = EditorUtil.GetManifestCacheFolder(configParam.Config.OutputRootDirectory,
                configParam.TargetPlatform);
            EditorUtil.CreateEmptyDirectory(folder);
            manifestParam = new ManifestParam(manifestParam.Manifest, folder);
            return ReturnCode.Success;
        }
    }
}