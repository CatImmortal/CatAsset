using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 循环依赖分析器
    /// </summary>
    public static class LoopDependencyAnalyzer
    {
        /// <summary>
        /// 分析资源的循环依赖
        /// </summary>
        public static void AnalyzeAsset(List<BundleBuildInfo> Bundles)
        {
            List<string> loops = new List<string>();

            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                if (bundleBuildInfo.IsRaw)
                {
                    //跳过原生资源包，因为加载原生资源时不会加载其依赖资源
                    continue;
                }

                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    CheckDependencies(assetBuildInfo.Name, loops, Util.GetDependencies);
                }
            }

            CheckLoops(loops);
        }


        /// <summary>
        /// 分析资源包的循环依赖
        /// </summary>
        public static void AnalyzeBundle(List<BundleBuildInfo> Bundles)
        {
            Dictionary<string, List<string>> bundleDependencies = GetBundleDependencies(Bundles);

            List<string> loops = new List<string>();

            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                if (bundleBuildInfo.IsRaw)
                {
                    //跳过原生资源包，因为加载原生资源时不会加载其依赖资源
                    continue;
                }

                CheckDependencies(bundleBuildInfo.RelativePath, loops,
                    (bundleName, _) =>
                    {
                        bundleDependencies.TryGetValue(bundleName, out List<string> dependencies);
                        return dependencies;
                    });
            }

            CheckLoops(loops);
        }

        /// <summary>
        /// 获取资源包及其依赖的其他资源包记录
        /// </summary>
        private static Dictionary<string, List<string>> GetBundleDependencies(List<BundleBuildInfo> Bundles)
        {
            //资源名与所属资源包
            Dictionary<string, BundleBuildInfo> assetToBundle =
                new Dictionary<string, BundleBuildInfo>();

            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                if (bundleBuildInfo.IsRaw)
                {
                    //跳过原生资源包，因为加载原生资源时不会加载其依赖资源
                    continue;
                }

                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    assetToBundle.Add(assetBuildInfo.Name, bundleBuildInfo);
                }
            }

            //资源包名与其所依赖的资源包
            Dictionary<string, List<string>> bundleDependency = new Dictionary<string, List<string>>();

            foreach (BundleBuildInfo bundleBuildInfo in Bundles)
            {
                if (bundleBuildInfo.IsRaw)
                {
                    //跳过原生资源包，因为加载原生资源时不会加载其依赖资源
                    continue;
                }

                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    List<string> dependencies = Util.GetDependencies(assetBuildInfo.Name, false);

                    if (dependencies == null)
                    {
                        continue;
                    }
                    
                    foreach (string dependency in dependencies)
                    {
                        BundleBuildInfo dependencyBundle = assetToBundle[dependency];

                        if (!bundleDependency.TryGetValue(bundleBuildInfo.RelativePath, out List<string> list))
                        {
                            list = new List<string>();
                            bundleDependency.Add(bundleBuildInfo.RelativePath, list);
                        }

                        if (!bundleBuildInfo.Equals(dependencyBundle) && !list.Contains(dependencyBundle.RelativePath))
                        {
                            list.Add(dependencyBundle.RelativePath);
                        }
                    }
                }
            }

            return bundleDependency;
        }

        /// <summary>
        /// 检查依赖
        /// </summary>
        private static void CheckDependencies(string name, List<string> loops,
            Func<string, bool, List<string>> GetDependenciesFunc)
        {
            List<string> depChainList = new List<string>(); //记录依赖链的列表
            depChainList.Add(name);

            HashSet<string> depChainSet = new HashSet<string>(); //记录依赖链的集合
            depChainSet.Add(name);

            if (!RecursiveCheckDependencies(name, depChainSet, depChainList, GetDependenciesFunc))
            {
                string loopLog = "     ";
                HashSet<string> lookedDepSet = new HashSet<string>(); //记录在依赖链上出现过资源
                foreach (string dep in depChainList)
                {
                    loopLog += dep + "\n->";

                    if (lookedDepSet.Contains(dep))
                    {
                        //重复出现过 是依赖环的入口
                        loopLog = loopLog.Replace(dep, "<color=#ff0000>" + dep + "</color>");
                    }
                    else
                    {
                        lookedDepSet.Add(dep);
                    }
                }

                loopLog += "\n--------------------";
                loops.Add(loopLog);
            }
        }


        /// <summary>
        /// 递归检查依赖
        /// </summary>
        private static bool RecursiveCheckDependencies(string name, HashSet<string> depChainSet,
            List<string> depChainList, Func<string, bool, List<string>> GetDependenciesFunc)
        {
            //获取所有直接依赖
            List<string> dependencies = GetDependenciesFunc(name, false);

            if (dependencies == null)
            {
                //没有依赖 直接返回true了
                return true;
            }

            //递归检查依赖
            foreach (string item in dependencies)
            {
                if (depChainSet.Contains(item))
                {
                    //当前层级的资源有依赖到上一层级的资源 意味着出现循环依赖了
                    depChainList.Add(item);
                    return false;
                }

                depChainList.Add(item);
                depChainSet.Add(item);
                if (!RecursiveCheckDependencies(item, depChainSet, depChainList, GetDependenciesFunc))
                {
                    return false;
                }

                depChainList.RemoveAt(depChainList.Count - 1);
                depChainSet.Remove(item);
            }

            //返回上一层前 把直接依赖都从set中移除
            foreach (string item in dependencies)
            {
                depChainSet.Remove(item);
            }

            return true;
        }

        /// <summary>
        /// 检查是否有循环依赖的记录
        /// </summary>
        private static void CheckLoops(List<string> loops)
        {
            if (loops.Count > 0)
            {
                EditorUtility.DisplayDialog("提示", "检测到循环依赖,请查看控制台Log", "确认");
                string log = "检测到循环依赖：\n";
                foreach (string item in loops)
                {
                    log += item + "\n";
                }

                Debug.LogError(log);
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "未检测到循环依赖", "确认");
            }
        }
    }
}