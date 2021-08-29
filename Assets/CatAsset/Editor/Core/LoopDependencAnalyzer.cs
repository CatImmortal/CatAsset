using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CatAsset.Editor
{
    /// <summary>
    /// 循环依赖分析器
    /// </summary>
    public static class LoopDependencAnalyzer
    {

        /// <summary>
        /// 分析循环依赖
        /// </summary>
        public static void AnalyzeLoopDependenc(List<AssetBundleBuild> abBuildList)
        {
            List<string> loop = new List<string>();
            foreach (var item in abBuildList)
            {
                foreach (var item2  in item.assetNames)
                {
                    List<string> depLink = new List<string>();  //记录依赖链的列表
                    depLink.Add(item2);
                    if (!GetDependencies(item2,new HashSet<string>(),depLink))
                    {
                        string loopLog = "     ";
                        HashSet<string> depLinkSet = new HashSet<string>();
                        foreach (string dep in depLink)
                        {
                            loopLog += dep + "\n->";

                            if (depLinkSet.Contains(dep))
                            {
                                loopLog = loopLog.Replace(dep, "<color=#ff0000>" + dep + "</color>");
                            }
                            else
                            {
                               
                                depLinkSet.Add(dep);
                            }

                            

                            
                        }
                        loopLog += "\n--------------------";
                        loop.Add(loopLog);
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

        private static bool GetDependencies(string assetName,HashSet<string> depSet,List<string> depLink)
        {
            //获取所有直接依赖
            string[] dependencies = Util.GetDependencies(assetName, false);

            
            //把直接依赖都放进set中
            foreach (string item in dependencies)
            {
                if (depSet.Contains(item))
                {
                    //当前层的资源有依赖到上一层的资源 意味着出现循环依赖了
                    depLink.Add(item);
                    return false;
                }

                depSet.Add(item);
            }

            //递归依赖
            foreach (string item in dependencies)
            {
                depLink.Add(item);
                if (!GetDependencies(item, depSet,depLink))
                {
                    return false;
                }
                depLink.RemoveAt(depLink.Count - 1);
            }

            //把直接依赖都从set中移除
            foreach (string item in dependencies)
            {
                depSet.Remove(item);
            }

            return true;
        }
    }
}

