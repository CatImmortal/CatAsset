using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建配置
    /// </summary>
    [CreateAssetMenu]
    public class BundleBuildConfigSO : ScriptableObject
    {
        /// <summary>
        /// 资源清单版本号
        /// </summary>
        public int ManifestVersion = 1;
        
        /// <summary>
        /// 资源包构建目标平台
        /// </summary>
        public List<BuildTarget> TargetPlatforms;
        
        /// <summary>
        /// 资源包构建设置
        /// </summary>
        public BuildAssetBundleOptions Options = BuildAssetBundleOptions.ChunkBasedCompression
                                                 | BuildAssetBundleOptions.DisableLoadAssetByFileName
                                                 | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
        
        /// <summary>
        /// 资源包构建输出目录
        /// </summary>
        public string OutputPath = "./AssetBundles";
        
        /// <summary>
        /// 是否进行冗余资源分析
        /// </summary>
        public bool IsRedundancyAnalyze = true;
        
        /// <summary>
        /// 资源包构建目标平台只有1个时，在资源包构建完成后是否将其复制到只读目录下
        /// </summary>
        public bool IsCopyToReadOnlyPath = true;

        /// <summary>
        /// 要复制到只读目录下的资源组，以分号分隔
        /// </summary>
        public string CopyGroup = Util.DefaultGroup;

        /// <summary>
        /// 资源包构建目录列表
        /// </summary>
        public List<BundleBuildDirectory> Directories;

        /// <summary>
        /// 资源包构建信息列表
        /// </summary>
        public List<BundleBuildInfo> Bundles;

        /// <summary>
        /// 资源包构建规则名->资源包构建规则接口实例
        /// </summary>
        private Dictionary<string, IBundleBuildRule> ruleDict = new Dictionary<string, IBundleBuildRule>();

        /// <summary>
        /// 刷新资源包构建信息
        /// </summary>
        public void RefreshBundleBuildInfos()
        {
           
            Bundles.Clear();
            float stepNum = 6f;
            
            EditorUtility.DisplayProgressBar("刷新资源包构建信息","初始化资源包构建规则...",1/stepNum);
            //初始化资源包构建规则
            InitRuleDict();

            EditorUtility.DisplayProgressBar("刷新资源包构建信息","初始化资源包构建信息...",2/stepNum);
            //根据构建规则初始化资源包构建信息
            InitBundleBuildInfo(false);

            EditorUtility.DisplayProgressBar("刷新资源包构建信息","将隐式依赖都转换为显式构建资源...",3/stepNum);
            //将隐式依赖都转换为显式构建资源
            ImplicitDependencyToExplicitBuildAsset();

            //上一步执行后可能出现同一个隐式依赖被转换为了不同资源包的显式构建资源
            //因此可能出现了资源冗余的情况

            if (IsRedundancyAnalyze)
            {
                EditorUtility.DisplayProgressBar("刷新资源包构建信息","进行冗余资源分析...",4/stepNum);
                //进行冗余资源分析
                RedundancyAssetAnalyze();
            }

            EditorUtility.DisplayProgressBar("刷新资源包构建信息","分割场景资源包中的非场景资源...",5/stepNum);
            //在将隐式依赖转换为显式构建资源后，可能出现场景资源和非场景资源被放进了同一个资源包的情况
            //而这是Unity不允许的，会在BuildBundle时报错，所以需要在这一步将其拆开
            SplitSceneBundle();

            EditorUtility.DisplayProgressBar("刷新资源包构建信息","初始化原生资源包构建信息...",6/stepNum);
            //根据构建规则初始化原生资源包构建信息
            InitBundleBuildInfo(true);
            
            //最后给资源包列表排下序
            Bundles.Sort();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            
            EditorUtility.ClearProgressBar();
            Debug.Log("资源包构建信息刷新完毕");
            
            
        }

        /// <summary>
        /// 初始化资源包构建规则字典
        /// </summary>
        private void InitRuleDict()
        {
            Type[] types = typeof(BundleBuildConfigSO).Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsInterface && typeof(IBundleBuildRule).IsAssignableFrom(type) &&
                    !ruleDict.ContainsKey(type.Name))
                {
                    IBundleBuildRule rule = (IBundleBuildRule) Activator.CreateInstance(type);
                    ruleDict.Add(type.Name, rule);
                }
            }
        }

        /// <summary>
        /// 根据构建规则获取初始的资源包构建信息列表
        /// </summary>
        private void InitBundleBuildInfo(bool isRaw)
        {
            for (int i = 0; i < Directories.Count; i++)
            {
                BundleBuildDirectory bundleBuildDirectory = Directories[i];

                IBundleBuildRule rule = ruleDict[bundleBuildDirectory.BuildRuleName];
                if (rule.IsRaw == isRaw)
                {
                    List<BundleBuildInfo> bundles = rule.GetBundleList(bundleBuildDirectory);
                    Bundles.AddRange(bundles);
                }
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
                    string assetName = assetBuildInfo.Name;
                    explicitBuildAssetSet.Add(assetName);
                }
            }


            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                List<string> implicitDependencies = new List<string>();

                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    string assetName = assetBuildInfo.Name;

                    //检查依赖列表
                    List<string> dependencies = Util.GetDependencies(assetName);
                    if (dependencies != null)
                    {
                        foreach (string dependency in dependencies)
                        {
                            if (!explicitBuildAssetSet.Contains(dependency))
                            {
                                //被显式构建资源依赖，并且没有被显式构建的，就是隐式依赖
                                implicitDependencies.Add(dependency);
                            }
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
        /// 冗余资源分析
        /// </summary>
        private void RedundancyAssetAnalyze()
        {
            List<BundleBuildInfo> redundancyBundles = RedundancyAssetAnalyzer.Analyze(Bundles);
            foreach (BundleBuildInfo redundancyBundle in redundancyBundles)
            {
                Bundles.Add(redundancyBundle);
            }
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
                    newBundles.Add(bundleBuildInfo);
                    continue;
                }

                List<AssetBuildInfo> sceneAssets = new List<AssetBuildInfo>(); //场景资源
                List<AssetBuildInfo> normalAssets = new List<AssetBuildInfo>(); //非场景资源

                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    string assetName = assetBuildInfo.Name;
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
                    //没有混合场景资源和非场景资源 直接跳过了
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
                string bundleName = $"{splitNames[0]}_res.{splitNames[1]}";
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

        /// <summary>
        /// 获取用于构建资源包的AssetBundleBuild列表
        /// </summary>
        public List<AssetBundleBuild> GetAssetBundleBuilds()
        {
            List<AssetBundleBuild> result = new List<AssetBundleBuild>();
            foreach (BundleBuildInfo bundleBuildInfo in GetNormalBundleBuilds())
            {
                AssetBundleBuild bundleBuild = bundleBuildInfo.GetAssetBundleBuild();
                result.Add(bundleBuild);
            }
            return result;
        }

        /// <summary>
        /// 获取普通资源包构建信息列表
        /// </summary>
        public List<BundleBuildInfo> GetNormalBundleBuilds()
        {
            return GetBundleBuilds(false);
        }
        
        /// <summary>
        /// 获取原生资源包构建信息列表
        /// </summary>
        public List<BundleBuildInfo> GetRawBundleBuilds()
        {
            return GetBundleBuilds(true);
        }
        
        /// <summary>
        /// 获取资源包构建信息列表
        /// </summary>
        private List<BundleBuildInfo> GetBundleBuilds(bool isRaw)
        {
            List<BundleBuildInfo> result = new List<BundleBuildInfo>();
            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                if (bundleBuildInfo.IsRaw == isRaw)
                {
                    result.Add(bundleBuildInfo);
                }
                
            }
            return result;
        }

    }
}

