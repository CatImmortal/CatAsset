using System;
using System.Collections.Generic;
using System.Diagnostics;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;


namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建配置
    /// </summary>
    [CreateAssetMenu]
    public class BundleBuildConfigSO : ScriptableObject
    {

        private static BundleBuildConfigSO instance;

        public static BundleBuildConfigSO Instance
        {
            get
            {
                if (instance == null)
                {
                    string[] guids = AssetDatabase.FindAssets($"t:{nameof(BundleBuildConfigSO)}");
                    if (guids.Length == 0)
                    {

                        BundleBuildConfigSO so = CreateInstance<BundleBuildConfigSO>();
                        string path = $"Assets/{nameof(BundleBuildConfigSO)}.asset";
                        AssetDatabase.CreateAsset(so,path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Debug.Log($"创建{nameof(BundleBuildConfigSO)} 路径：{path}");
                        instance = so;
                    }
                    else
                    {
                        string path;
                        if (guids.Length > 1)
                        {
                            Debug.LogWarning($"{nameof(BundleBuildConfigSO)}数量大于1");
                            foreach (string guid in guids)
                            {
                                path = AssetDatabase.GUIDToAssetPath(guid);
                                Debug.LogWarning(path);
                            }
                        }

                        path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        instance = AssetDatabase.LoadAssetAtPath<BundleBuildConfigSO>(path);
                    }

                }

                return instance;
            }
        }

        /// <summary>
        /// 资源清单版本号
        /// </summary>
        public int ManifestVersion = 1;

        /// <summary>
        /// 资源包构建目标平台
        /// </summary>
        public List<BuildTarget> TargetPlatforms = new List<BuildTarget>();

        /// <summary>
        /// 资源包构建设置
        /// </summary>
        public BundleBuildOptions Options = BundleBuildOptions.WriteLinkXML;

        /// <summary>
        /// 资源包压缩全局压缩设置
        /// </summary>
        public BundleCompressOptions GlobalCompress = BundleCompressOptions.LZ4;
        
        /// <summary>
        /// 资源包构建输出目录
        /// </summary>
        public string OutputPath = "./Library/AssetBundles";

        /// <summary>
        /// 资源包构建目标平台只有1个时，在资源包构建完成后是否将其复制到只读目录下
        /// </summary>
        public bool IsCopyToReadOnlyDirectory = true;

        /// <summary>
        /// 要复制到只读目录下的资源组，以分号分隔
        /// </summary>
        public string CopyGroup = GroupInfo.DefaultGroup;

        /// <summary>
        /// 资源包构建目录列表
        /// </summary>
        public List<BundleBuildDirectory> Directories = new List<BundleBuildDirectory>();

        /// <summary>
        /// 资源包构建信息列表
        /// </summary>
        public List<BundleBuildInfo> Bundles = new List<BundleBuildInfo>();

        /// <summary>
        /// 资源包构建规则名 -> 资源包构建规则接口实例
        /// </summary>
        private Dictionary<string, IBundleBuildRule> ruleDict = new Dictionary<string, IBundleBuildRule>();

        /// <summary>
        /// 目录名 -> 构建目录对象
        /// </summary>
        public Dictionary<string, BundleBuildDirectory> DirectoryDict =new Dictionary<string, BundleBuildDirectory>();

        /// <summary>
        /// 资源名 -> 资源包构建信息
        /// </summary>
        public Dictionary<string, BundleBuildInfo> AssetToBundleDict = new Dictionary<string, BundleBuildInfo>();

        /// <summary>
        /// 资源包数量
        /// </summary>
        public int BundleCount;
        
        /// <summary>
        /// 资源数量
        /// </summary>
        public int AssetCount;

        /// <summary>
        /// 刷新资源包构建信息
        /// </summary>
        public void RefreshBundleBuildInfos()
        {

            Bundles.Clear();

            float stepNum = 6f;
            int curStep = 1;

            void ProfileTime(Action action,Stopwatch sw,string name)
            {
                // long oldMemory = Profiler.GetTotalAllocatedMemoryLong();
                // sw.Restart();
                action?.Invoke();
                // sw.Stop();
                // long newMemory = Profiler.GetTotalAllocatedMemoryLong();
                // long usedMemory = newMemory - oldMemory;
                // if (usedMemory <= 0)
                // {
                //     usedMemory = 0;
                // }
                
                //Debug.Log($"【{name}】 执行完毕，耗时：{sw.Elapsed.TotalSeconds}秒，使用内存：{RuntimeUtil.GetByteLengthDesc((ulong)usedMemory)}");
                //sw.Reset();
            }

            try
            {
                Stopwatch sw = new Stopwatch();

                //初始化资源包构建规则
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "初始化资源包构建规则...", curStep / stepNum);
                ProfileTime(InitRuleDict,sw,"初始化资源包构建规则");
                curStep++;


                //根据构建规则初始化资源包构建信息
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "初始化资源包构建信息...", curStep / stepNum);
                ProfileTime((() => {InitBundleBuildInfo(false);}),sw,"初始化资源包构建信息");
                curStep++;
                
                //将隐式依赖都转换为显式构建资源
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "将隐式依赖都转换为显式构建资源...", curStep / stepNum);
                ProfileTime(ImplicitDependencyToExplicitBuildAsset,sw,"将隐式依赖都转换为显式构建资源");
                curStep++;

                //上一步执行后可能出现同一个隐式依赖被转换为了不同资源包的显式构建资源
                //因此可能出现了资源冗余的情况
                
                //进行冗余资源分析
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "进行冗余资源分析...", curStep / stepNum);
                ProfileTime(RedundancyAssetAnalyze,sw,"进行冗余资源分析");
                curStep++;

                //在将隐式依赖转换为显式构建资源后，可能出现场景资源和非场景资源被放进了同一个资源包的情况
                //而这是Unity不允许的，会在BuildBundle时报错，所以需要在这一步将其拆开
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "分割场景资源包中的非场景资源...", curStep / stepNum);
                ProfileTime(SplitSceneBundle,sw,"分割场景资源包中的非场景资源");
                curStep++;

                //根据构建规则初始化原生资源包构建信息
                //如果出现普通资源包中的资源依赖原生资源，那么需要冗余一份原生资源到普通资源包中，因为本质上原生资源是没有资源包的
                //所以这里将原生资源包的构建放到最后处理，这样通过前面的隐式依赖转换为显式构建资源这一步就可以达成原生资源在依赖它的普通资源包中的冗余了
                EditorUtility.DisplayProgressBar("刷新资源包构建信息", "初始化原生资源包构建信息...", curStep / stepNum);
                ProfileTime((() => { InitBundleBuildInfo(true);}),sw,"初始化原生资源包构建信息");

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            //移除空资源包
            RemoveEmptyBundle();

            //刷新资源包的总资源长度
            RefreshBundleLength();

            //给资源包和资源列表排下序
            SortBundles();

            //刷新映射
            RefreshDirectoryDict();
            RefreshAssetToBundleDict();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();


            Debug.Log("资源包构建信息刷新完毕");
        }

        /// <summary>
        /// 获取资源包构建规则字典
        /// </summary>
        /// <returns></returns>
       internal Dictionary<string, IBundleBuildRule> GetRuleDict()
        {
            if (ruleDict.Count == 0)
            {
                InitRuleDict();
            }

            return ruleDict;
        }

        /// <summary>
        /// 初始化资源包构建规则字典
        /// </summary>
        private void InitRuleDict()
        {
            ruleDict.Clear();
            List<IBundleBuildRule> rules = EditorUtil.GetAssignableTypeObjects<IBundleBuildRule>();
            foreach (IBundleBuildRule rule in rules)
            {
                ruleDict.Add(rule.GetType().Name,rule);
            }
        }

        /// <summary>
        /// 根据构建规则获取初始的资源包构建信息列表
        /// </summary>
        private void InitBundleBuildInfo(bool isRaw)
        {
            //降序遍历构建目录 实现非中序遍历效果 以支持嵌套构建目录
            List<BundleBuildDirectory> tempDirectories = new List<BundleBuildDirectory>();
            for (int i = Directories.Count - 1; i >= 0; i--)
            {
                tempDirectories.Add(Directories[i]);
            }
            
            //已被构建规则处理过的资源的集合
            HashSet<string> lookedAssets = new HashSet<string>();
            
            for (int i = 0; i < tempDirectories.Count; i++)
            {
                BundleBuildDirectory bundleBuildDirectory = tempDirectories[i];
                EditorUtility.DisplayProgressBar("初始化资源包构建信息", $"{bundleBuildDirectory.DirectoryName}", i / (tempDirectories.Count * 1.0f));
                IBundleBuildRule rule = ruleDict[bundleBuildDirectory.BuildRuleName];
                if (rule.IsRaw == isRaw)
                {
                    //获取根据构建规则形成的资源包构建信息列表
                    List<BundleBuildInfo> bundles = rule.GetBundleList(bundleBuildDirectory, lookedAssets);
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
                //隐式依赖集合
                //这里使用HashSet 防止出现当资源包A中的资源a,b，同时隐式依赖资源c时，c被implicitDependencies.Add两次导致重复的问题
                HashSet<string> implicitDependencies = new HashSet<string>();

                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    if (EditorUtil.NotDependencyAssetType.Contains(assetBuildInfo.Type))
                    {
                        //跳过不会依赖其他资源的资源类型
                        continue;
                    }
                    
                    //检查依赖列表
                    string assetName = assetBuildInfo.Name;
                    List<string> dependencies = EditorUtil.GetDependencies(assetName);

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
                    bundleBuildInfo.BundleName, bundleBuildInfo.Group, false,bundleBuildInfo.CompressOption);
                foreach (AssetBuildInfo sceneAsset in sceneAssets)
                {
                    sceneBundleBuildInfo.Assets.Add(sceneAsset);
                }

                newBundles.Add(sceneBundleBuildInfo);

                //重建非场景资源包
                string[] splitNames = bundleBuildInfo.BundleName.Split('.');
                string bundleName = $"{splitNames[0]}_res.{splitNames[1]}";
                BundleBuildInfo normalBundleBuildInfo = new BundleBuildInfo(bundleBuildInfo.DirectoryName, bundleName,
                    bundleBuildInfo.Group, false,bundleBuildInfo.CompressOption);
                foreach (AssetBuildInfo normalAsset in normalAssets)
                {
                    normalBundleBuildInfo.Assets.Add(normalAsset);
                }

                newBundles.Add(normalBundleBuildInfo);


            }

            Bundles = newBundles;
        }

        /// <summary>
        /// 移除空资源包
        /// </summary>
        private void RemoveEmptyBundle()
        {
            for (int i = Bundles.Count - 1; i >= 0; i--)
            {
                BundleBuildInfo bundleBuildInfo = Bundles[i];
                if (bundleBuildInfo.Assets.Count == 0)
                {
                    Bundles.RemoveAt(i);
                }
            }
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
        /// 排序资源包列表
        /// </summary>
        private void SortBundles()
        {
            Bundles.Sort();
            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                bundleBuildInfo.Assets.Sort();
            }
        }


        /// <summary>
        /// 刷新映射信息
        /// </summary>
        internal void RefreshDict()
        {
            RefreshDirectoryDict();
            RefreshAssetToBundleDict();
        }

        private void RefreshDirectoryDict()
        {
            DirectoryDict.Clear();
            foreach (BundleBuildDirectory item in Directories)
            {
                DirectoryDict[item.DirectoryName] = item;
            }
        }

        private void RefreshAssetToBundleDict()
        {
            AssetCount = 0;
            BundleCount = Bundles.Count;
            AssetToBundleDict.Clear();
            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    AssetCount++;
                    AssetToBundleDict.Add(assetBuildInfo.Name,bundleBuildInfo);
                }
            }
        }

        /// <summary>
        /// 检查资源包构建目录是否可被添加
        /// </summary>
        internal bool CanAddDirectory(string directoryName)
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

