using System.IO;
using CatAsset.Runtime;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 写入资源清单文件
    /// </summary>
    public class WriteManifestFile : IBuildTask
    {
        [InjectContext(ContextUsage.In)]
        private IManifestParam manifestParam;

        /// <inheritdoc />
        public int Version => 1;

        public ReturnCode Run()
        {
            string writeFolder = manifestParam.WriteFolder;
            CatAssetManifest manifest = manifestParam.Manifest;

            manifest.WriteFile(writeFolder,false);
            manifest.WriteFile(writeFolder,true);

            return ReturnCode.Success;
        }


    }
}
