using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 加密资源包
    /// </summary>
    public class EncryptBundles : IBuildTask
    {
        [InjectContext(ContextUsage.In)]
        private IManifestParam manifestParam;

        [InjectContext(ContextUsage.In)]
        private IBundleBuildParameters buildParam;
        
        [InjectContext(ContextUsage.In)] 
        private IBundleBuildConfigParam configParam;
        
        /// <inheritdoc />
        public int Version { get; }
        
        public ReturnCode Run()
        {
            if (configParam.TargetPlatform == BuildTarget.WebGL)
            {
                //WebGL平台不允许加密 因为无法偏移加密 而异或加密内存开销太高会爆WebGL平台的内存
                return ReturnCode.SuccessNotRun;
            }
            
            CatAssetManifest manifest = manifestParam.Manifest;

            string outputFolder = ((BundleBuildParameters) buildParam).OutputFolder;

            foreach (BundleManifestInfo bundleManifestInfo in manifest.Bundles)
            {
                if (bundleManifestInfo.EncryptOption == BundleEncryptOptions.NotEncrypt)
                {
                    //不加密 跳过
                    continue;
                }

                string filePath =
                    RuntimeUtil.GetRegularPath(Path.Combine(outputFolder, bundleManifestInfo.BundleIdentifyName));
                
                switch (bundleManifestInfo.EncryptOption)
                {
                    case BundleEncryptOptions.Offset:
                        EncryptUtil.EncryptOffset(filePath);
                        break;
                    
                    case BundleEncryptOptions.XOr:
                        EncryptUtil.EncryptXOr(filePath);
                        break;
                }
            }

            return ReturnCode.Success;
        }

    }
}