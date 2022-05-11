using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建配置
    /// </summary>
    [CreateAssetMenu()]
    public class BundleBuildConfigSO : ScriptableObject
    {
        /// <summary>
        /// 资源包构建目录列表
        /// </summary>
        public List<BundleBuildDirectory> Directories;

        /// <summary>
        /// 资源包构建信息列表
        /// </summary>
        public List<BundleBuildInfo> Bundles;

        /// <summary>
        /// 是否进行冗余资源分析
        /// </summary>
        public bool IsRedundancyAnalyze = true;
        
        /// <summary>
        /// 资源包构建规则名->资源包构建规则接口实例
        /// </summary>
        private Dictionary<string, IBundleBuildRule> ruleDict = new Dictionary<string, IBundleBuildRule>();

        /// <summary>
        /// 刷新资源包构建信息
        /// </summary>
        public void RefreshBundleBuildInfo()
        {
            //初始化资源包构建规则
            InitRuleDict();
            
            //根据构建规则初始化初资源包构建信息
            InitBundleBuildInfo();

            //将隐式依赖都转换为显式构建资源
            ImplicitDependencyToExplicitBuildAsset();
            
            //上一步执行后可能出现同一个隐式依赖被转换为了不同资源包的显式构建资源
            //因此可能出现了资源冗余的情况
            
            if (IsRedundancyAnalyze)
            {
                //进行冗余资源分析
                RedundancyAnalyze();
            }
            
            //在将隐式依赖转换为显式构建资源后，可能出现场景资源和非场景资源被放进了同一个资源包的情况
            //而这是Unity不允许的，会在BuildBundle时报错，所以需要在这一步将其拆开
            SplitSceneBundle();
           
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 初始化资源包构建规则字典
        /// </summary>
        private void InitRuleDict()
        {
            Type[] types = typeof(BundleBuildConfigSO).Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsInterface && typeof(IBundleBuildRule).IsAssignableFrom(type) && !ruleDict.ContainsKey(type.Name))
                {
                    IBundleBuildRule rule = (IBundleBuildRule)Activator.CreateInstance(type);
                    ruleDict.Add(type.Name,rule);
                }
            }
        }

        /// <summary>
        /// 根据构建规则获取初始的资源包构建信息列表
        /// </summary>
        private void InitBundleBuildInfo()
        {
            Bundles.Clear();
            for (int i = 0; i < Directories.Count; i++)
            {
                BundleBuildDirectory bundleBuildDirectory = Directories[i];
                
                IBundleBuildRule rule = ruleDict[bundleBuildDirectory.BuildRuleName];
                List<BundleBuildInfo> bundles = rule.GetBundleList(bundleBuildDirectory);
                Bundles.AddRange(bundles);
            }
        }
        
        /// <summary>
        /// 将隐式依赖转为显式构建资源
        /// </summary>
        private void ImplicitDependencyToExplicitBuildAsset()
        {
            //被显式构建的资源集合
            HashSet<string> explicitBuildAssetSet = new HashSet<string>();
            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    string assetName = assetBuildInfo.AssetName;
                    explicitBuildAssetSet.Add(assetName);
                }
            }
            
            
            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                if (bundleBuildInfo.IsRaw)
                {
                    //原生资源包不进行处理
                    //因为原生资源包本质是个虚拟资源包，所以bundleBuildInfo.Assets列表里只能有1个原生资源存在
                    continue;
                }
                
                List<string> implicitDependencies = new List<string>();

                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    string assetName = assetBuildInfo.AssetName;
                    
                    //检查依赖列表
                    string[] dependencies = Util.GetDependencies(assetName);
                    foreach (string dependency in dependencies)
                    {
                        if (!explicitBuildAssetSet.Contains(dependency))
                        {
                            //被显式构建资源依赖，并且没有被显式构建的，就是隐式依赖
                            implicitDependencies.Add(dependency);
                        }
                    }
                }
                
                //将隐式依赖转为显式构建资源
                foreach (string implicitDependency in implicitDependencies)
                {
                    bundleBuildInfo.Assets.Add(new AssetBuildInfo(implicitDependency));
                }
            }
        }

        /// <summary>
        /// 冗余分析，将在不同资源包中的相同资源单独提取出来到冗余资源包中
        /// </summary>
        private void RedundancyAnalyze()
        {
            
        }
        
        /// <summary>
        /// 分割场景资源包中的非场景资源
        /// </summary>
        private void SplitSceneBundle()
        {
            List<BundleBuildInfo> newBundles = new List<BundleBuildInfo>();

            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                if (bundleBuildInfo.IsRaw)
                {
                    continue;
                }
                
                List<AssetBuildInfo> sceneAssets = new List<AssetBuildInfo>(); //场景资源
                List<AssetBuildInfo> normalAssets = new List<AssetBuildInfo>(); //非场景资源
                
                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    string assetName = assetBuildInfo.AssetName;
                    if (assetName.EndsWith(".unity"))
                    {
                        sceneAssets.Add(assetBuildInfo);
                    }
                    else
                    {
                        normalAssets.Add(assetBuildInfo);
                    }
                }

                if (sceneAssets.Count == 0 || normalAssets.Count == 0)
                {
                    //没有混合场景资源和非场景资源
                    newBundles.Add(bundleBuildInfo);
                    continue;
                }
                
                //场景资源和非场景资源被混进同一个资源包里了，需要拆分
                
                //重建场景资源包
                BundleBuildInfo sceneBundleBuildInfo = new BundleBuildInfo(bundleBuildInfo.DirectoryName,
                    bundleBuildInfo.BundleName, bundleBuildInfo.Group, false);
                foreach (AssetBuildInfo sceneAsset in sceneAssets)
                {
                    sceneBundleBuildInfo.Assets.Add(sceneAsset);
                }
                newBundles.Add(sceneBundleBuildInfo);
                
                //重建非场景资源包
                string[] splitNames = bundleBuildInfo.BundleName.Split('.');
                string bundleName =  $"{splitNames[0]}_res.{splitNames[1]}";
                BundleBuildInfo normalBundleBuildInfo = new BundleBuildInfo(bundleBuildInfo.DirectoryName, bundleName,
                    bundleBuildInfo.Group, false);
                foreach (AssetBuildInfo normalAsset in normalAssets)
                {
                    normalBundleBuildInfo.Assets.Add(normalAsset);
                }
                newBundles.Add(normalBundleBuildInfo);
                

            }

            Bundles = newBundles;
        }
        
        /// <summary>
        /// 检查资源包构建目录是否可被添加
        /// </summary>
        public bool CanAddDirectory(string directoryName)
        {
            for (int i = 0; i < Directories.Count; i++)
            {
                BundleBuildDirectory bundleBuildDirectory = Directories[i];

                if (directoryName == bundleBuildDirectory.DirectoryName)
                {
                    //同名目录不能重复添加
                    return false;
                }
            }

            return true;
        }
        
        
    }
}

