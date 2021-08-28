using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CatAsset.Editor
{
    public static class LoopDependencAnalyzer
    {
        [MenuItem("CatAsset/测试循环依赖检查")]
        public static void TestLoop()
        {
            CheckLoopDependenc(Util.PkgRuleCfg.GetAssetBundleBuildList());
        }

        /// <summary>
        /// 检测循环依赖
        /// </summary>
        public static void CheckLoopDependenc(List<AssetBundleBuild> abBuildList)
        {
            List<string> loop = new List<string>();
            foreach (var item in abBuildList)
            {
                foreach (var item2  in item.assetNames)
                {
                    if (!GetDependencies(item2,new HashSet<string>()))
                    {
                        loop.Add(item2);
                    }
                }
            }

            if (loop.Count > 0)
            {
                EditorUtility.DisplayDialog("提示", "检测到循环依赖,请查看控制台Log", "确认");
                string log = "检测到循环依赖：\n";
                foreach (var item in loop)
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

        private static bool GetDependencies(string assetName,HashSet<string> dependenciesSet)
        {
            //获取所有直接依赖
            string[] dependencies = Util.GetDependencies(assetName, false);

            
            //把直接依赖都放进set中
            foreach (string item in dependencies)
            {
                if (dependenciesSet.Contains(item))
                {
                    //当前层的资源有依赖到上一层的资源 意味着出现循环依赖了
                    return false;
                }

                dependenciesSet.Add(item);
            }

            //递归依赖
            foreach (string item in dependencies)
            {
                if (!GetDependencies(item, dependenciesSet))
                {
                    return false;
                }
            }

            //把直接依赖都从set中移除
            foreach (string item in dependencies)
            {
                dependenciesSet.Remove(item);
            }

            return true;
        }
    }
}

