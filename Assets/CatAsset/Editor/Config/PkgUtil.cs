using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CatAsset.Editor
{
    public static class PkgUtil
    {
        private static PackageConfig pkgCfg;
        private static PackageRuleConfig pkgRuleCfg;

        /// <summary>
        /// 打包配置
        /// </summary>
        public static PackageConfig PkgCfg
        {
            get
            {
                if (pkgCfg == null)
                {
                    pkgCfg = GetConfigAsset<PackageConfig>();
                }
                return pkgCfg;
            }
        }

        /// <summary>
        /// 打包规则配置
        /// </summary>
        public static PackageRuleConfig PkgRuleCfg
        {
            get
            {
                if (pkgRuleCfg == null)
                {
                    pkgRuleCfg = GetConfigAsset<PackageRuleConfig>();
                }
                return pkgRuleCfg;
            }
        }


        /// <summary>
        /// 创建配置
        /// </summary>
        public static T CreateConfigAsset<T>() where T : ScriptableObject
        {
            string typeName = typeof(T).Name;
            string[] paths = AssetDatabase.FindAssets("t:" + typeName);
            if (paths.Length >= 1)
            {
                string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                EditorUtility.DisplayDialog("警告", $"已存在{typeName}，路径:{path}", "确认");
                return null;
            }

            T cfg = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(cfg, $"Assets/{typeName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("提示", $"{typeName}创建完毕，路径:Assets/{typeName}.asset", "确认");

            return cfg;
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        private static T GetConfigAsset<T>() where T : ScriptableObject
        {

            string typeName = typeof(T).Name;
            string[] paths = AssetDatabase.FindAssets("t:" + typeName);
            if (paths.Length == 0)
            {
                Debug.LogError("不存在" + typeName);
                return null;
            }
            if (paths.Length > 1)
            {
                Debug.LogError(typeName + "数量大于1");
                return null;

            }
            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            T config = AssetDatabase.LoadAssetAtPath<T>(path);

            return config;
        }

        /// <summary>
        /// 获取排除了自身和csharp代码文件的依赖资源列表
        /// </summary>
        public static string[] GetDependencies(string assetName,bool recursive = true)
        {
            string[] dependencies = AssetDatabase.GetDependencies(assetName,recursive);

            if (dependencies.Length == 0)
            {
                return dependencies;
            }

            List<string> result = new List<string>();
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependencyName = dependencies[i];
                if (dependencyName == assetName || dependencyName.EndsWith(".cs"))
                {
                    continue;
                }

                result.Add(dependencyName);
            }

            return result.ToArray();
        }





    }

}
