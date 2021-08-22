using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 打包器
    /// </summary>
    public static class Packager
    {
        [MenuItem("CatAsset/打包AssetBundle")]
        public static void BuildAssetBundle()
        {
            PackageRuleConfig config = Util.GetPackageRuleConfig();

            List<AssetBundleBuild> abBuildList = new List<AssetBundleBuild>();

            foreach (PackageRule rule in config.Rules)
            {
                Func<string, AssetBundleBuild[]> func = AssetCollectFuncs.FuncDict[rule.Mode];
                AssetBundleBuild[] abBuilds = func(rule.Directory);
                abBuildList.AddRange(abBuilds);

            }

            //foreach (var item in abBuildList)
            //{
            //    Debug.Log(item.assetBundleName);
            //    foreach (var item2 in item.assetNames)
            //    {
            //        Debug.Log(item2);
            //    }

            //    Debug.Log("-------------");
            //}



            BuildPipeline.BuildAssetBundles(Directory.GetCurrentDirectory() + "/AssetBundleOutput", abBuildList.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            
        }

      
    }

}
