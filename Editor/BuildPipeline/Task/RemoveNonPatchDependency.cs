using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 移除补丁资源依赖列表中的非补丁资源，以防止资源内存冗余
    /// </summary>
    public class RemoveNonPatchDependency : IBuildTask
    {
        public int Version { get; }
        
        [InjectContext(ContextUsage.In)]
        private IManifestParam manifestParam;
        
        public ReturnCode Run()
        {
            HashSet<string> patchAssets = new HashSet<string>();
            foreach (BundleManifestInfo bundleManifestInfo in manifestParam.Manifest.Bundles)
            {
                foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
                {
                    patchAssets.Add(assetManifestInfo.Name);
                }
            }
            
            foreach (BundleManifestInfo bundleManifestInfo in manifestParam.Manifest.Bundles)
            {
                foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
                {
                    for (int i = assetManifestInfo.Dependencies.Count - 1; i >= 0; i--)
                    {
                        string dependency = assetManifestInfo.Dependencies[i];
                        if (!patchAssets.Contains(dependency))
                        {
                            assetManifestInfo.Dependencies.RemoveAt(i);
                            Debug.Log($"移除{assetManifestInfo}的依赖列表中的非补丁资源{dependency}");
                        }
                    }
                }
            }

            return ReturnCode.Success;
        }
    }
}