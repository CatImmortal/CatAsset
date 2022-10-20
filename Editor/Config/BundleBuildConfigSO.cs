using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// 是否进行共享资源分析
        /// </summary>
        public bool IsSharedAssetAnalyze = true;

        /// <summary>
        /// 资源包构建目标平台只有1个时，在资源包构建完成后是否将其复制到只读目录下
        /// </summary>
        public bool IsCopyToReadOnlyDirectory = true;

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
        /// 资源名 -> 资源构建信息
        /// </summary>
        private Dictionary<string, AssetBuildInfo> assetBuildInfoDict = new Dictionary<string, AssetBuildInfo>();

        /// <summary>
        /// 资源包相对路径 -> 资源包构建信息
        /// </summary>
        private Dictionary<string, BundleBuildInfo> bundleBuildInfoDict = new Dictionary<string, BundleBuildInfo>();

        /// <summary>
        /// 刷新资源包构建信息
        /// </summary>
        public void RefreshBundleBuildInfos()
        {

            Bundles.Clear();

            float stepNum = 6f;
            int curStep = 1;

            try
            {
                //初始化资源包构建规则
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "初始化资源包构建规则...", curStep / stepNum);
                InitRuleDict();
                curStep++;

                //根据构建规则初始化资源包构建信息
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "初始化资源包构建信息...", curStep / stepNum);
                InitBundleBuildInfo(false);
                curStep++;

                //将隐式依赖都转换为显式构建资源
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "将隐式依赖都转换为显式构建资源...", curStep / stepNum);
                ImplicitDependencyToExplicitBuildAsset();
                curStep++;

                if (IsSharedAssetAnalyze)
                {
                    //进行共享资源分析
                    EditorUtility.DisplayProgressBar("刷新资源包构建信息", "进行共享资源分析...", curStep / stepNum);
                    SharedAssetAnalyze();
                }
                curStep++;

                //在将隐式依赖转换为显式构建资源后，可能出现场景资源和非场景资源被放进了同一个资源包的情况
                //而这是Unity不允许的，会在BuildBundle时报错，所以需要在这一步将其拆开
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "分割场景资源包中的非场景资源...", curStep / stepNum);
                SplitSceneBundle();
                curStep++;

                //根据构建规则初始化原生资源包构建信息
                //如果出现普通资源包中的资源依赖原生资源，那么需要冗余一份原生资源到普通资源包中，因为本质上原生资源是没有资源包的
                //所以这里将原生资源包的构建放到最后处理，这样通过前面的隐式依赖转换为显式构建资源这一步就可以达成原生资源在依赖它的普通资源包中的冗余了
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "初始化原生资源包构建信息...", curStep / stepNum);
                InitBundleBuildInfo(true);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                assetBuildInfoDict.Clear();
                bundleBuildInfoDict.Clear();
            }
            
            //筛选出资源数不为0的资源包
            Bundles = Bundles.Where((info => info.Assets.Count > 0)).ToList();
            
            //刷新资源包的总资源长度
            RefreshBundleLength();
            
            //最后给资源包和资源列表排下序
            Bundles.Sort();
            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                bundleBuildInfo.Assets.Sort();
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();


            Debug.Log("资源包构建信息刷新完毕");


        }

        /// <summary>
        /// 初始化资源包构建规则字典
        /// </summary>
        private void InitRuleDict()
        {
            ruleDict.Clear();
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IBundleBuildRule>();
            foreach (Type type in types)
            {
                IBundleBuildRule rule = (IBundleBuildRule) Activator.CreateInstance(type);
                ruleDict.Add(type.Name, rule);
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
                    //获取根据构建规则形成的资源包构建信息列表
                    List<BundleBuildInfo> bundles = rule.GetBundleList(bundleBuildDirectory);

                    //添加映射信息
                    foreach (BundleBuildInfo bundleBuildInfo in bundles)
                    {
                        bundleBuildInfoDict.Add(bundleBuildInfo.RelativePath,bundleBuildInfo);

                        foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                        {
                            assetBuildInfoDict.Add(assetBuildInfo.Name,assetBuildInfo);
                        }
                    }

                    Bundles.AddRange(bundles);
                }
            }


        }

        /// <summary>
        /// 将隐式依赖转为显式构建资源
        /// </summary>
        private void ImplicitDependencyToExplicitBuildAsset()
        {

            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                //隐式依赖集合
                HashSet<string> implicitDependencies = new HashSet<string>();

                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    string assetName = assetBuildInfo.Name;

                    //检查依赖列表
                    List<string> dependencies = Util.GetDependencies(assetName);
                    foreach (string dependency in dependencies)
                    {
                        if (!assetBuildInfoDict.ContainsKey(dependency))
                        {
                            //被显式构建资源所依赖，并且没有被显式构建的资源，就是隐式依赖
                            implicitDependencies.Add(dependency);
                        }
                    }
                }

                //将隐式依赖转为显式构建资源
                //如果A包和B包同时隐式依赖资源x，那么只会将x构建进其中一个包里，而不是2个资源包都有份
                foreach (string implicitDependency in implicitDependencies)
                {
                    AssetBuildInfo assetBuildInfo =
                        new AssetBuildInfo(implicitDependency, bundleBuildInfo.RelativePath);
                    bundleBuildInfo.Assets.Add(assetBuildInfo);
                    assetBuildInfoDict.Add(assetBuildInfo.Name,assetBuildInfo);
                }
            }
        }

        /// <summary>
        /// 冗余资源分析
        /// </summary>
        private void RedundancyAssetAnalyze()
        {
            // List<BundleBuildInfo> redundancyBundles = RedundancyAssetAnalyzer.Analyze(Bundles);
            // foreach (BundleBuildInfo redundancyBundle in redundancyBundles)
            // {
            //     Bundles.Add(redundancyBundle);
            // }
        }

        /// <summary>
        /// 共享资源分析
        /// </summary>
        private void SharedAssetAnalyze()
        {
            //若A包的资源x，还被B包依赖，那么x就是共享资源，需要被提取到share bundle中

            //共享资源构建信息的集合
            HashSet<AssetBuildInfo> sharedAssetBuildInfos = new HashSet<AssetBuildInfo>();

            foreach (AssetBuildInfo assetBuildInfo in assetBuildInfoDict.Values)
            {
                //检查依赖列表
                string assetName = assetBuildInfo.Name;
                List<string> dependencies = Util.GetDependencies(assetName,false);

                foreach (string dependency in dependencies)
                {
                    AssetBuildInfo dependencyAssetBuildInfo = assetBuildInfoDict[dependency];
                    if (dependencyAssetBuildInfo.BundleRelativePath != assetBuildInfo.BundleRelativePath)
                    {
                        sharedAssetBuildInfos.Add(dependencyAssetBuildInfo);
                    }
                }
            }

            //共享资源的资源包构建信息
            Dictionary<string, BundleBuildInfo> sharedBundleDict = new Dictionary<string, BundleBuildInfo>();

            foreach (AssetBuildInfo assetBuildInfo in sharedAssetBuildInfos)
            {
                //先把共享资源从原来的资源包中分离
                BundleBuildInfo oldBundleBuildInfo = bundleBuildInfoDict[assetBuildInfo.BundleRelativePath];
                oldBundleBuildInfo.Assets.Remove(assetBuildInfo);
                assetBuildInfo.BundleRelativePath = null;

                //使用共享资源所在目录作为资源包名
                string assetName = assetBuildInfo.Name;
                FileInfo fi = new FileInfo(assetName);
                string fileName = fi.Name;
                string parentDirectoryName = fi.Directory.Name;
                string bundleName = $"shared_{parentDirectoryName}.bundle";
                string directoryName = assetName.Replace("Assets/",string.Empty).Replace($"{parentDirectoryName}/{fileName}",String.Empty);
                directoryName = directoryName.TrimEnd('/');  //末尾可能多出来个/，要删除
                string bundleRelativePath = $"{directoryName}/{bundleName}";
                string group = Util.DefaultGroup;  //TODO:共享资源包先分进Base组，后续支持单资源包标记多资源组后再改

                if (!sharedBundleDict.TryGetValue(bundleRelativePath,out BundleBuildInfo bundleBuildInfo))
                {
                    bundleBuildInfo = new BundleBuildInfo(directoryName, bundleName, group, false);
                    Bundles.Add(bundleBuildInfo);
                    sharedBundleDict.Add(bundleRelativePath, bundleBuildInfo);
                    bundleBuildInfoDict.Add(bundleRelativePath,bundleBuildInfo);
                }

                assetBuildInfo.BundleRelativePath = bundleRelativePath;
                bundleBuildInfo.Assets.Add(assetBuildInfo);
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
        /// 刷新资源包的总资源长度
        /// </summary>
        private void RefreshBundleLength()
        {
            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                bundleBuildInfo.RefreshAssetsLength();
            }
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
                if (bundleBuild.assetNames.Length == 0)
                {
                    continue;
                }
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

