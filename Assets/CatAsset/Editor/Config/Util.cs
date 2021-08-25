using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CatAsset.Editor
{
    public static class Util
    {
        /// <summary>
        /// 打包配置
        /// </summary>
        public static PackageConfig PkgCfg
        {
            get;
            private set;
        }

        /// <summary>
        /// 打包规则配置
        /// </summary>
        public static PackageRuleConfig PkgRuleCfg
        {
            get;
            private set;
        }

        static Util()
        {
            PkgCfg = GetConfigAsset<PackageConfig>();
            PkgRuleCfg = GetConfigAsset<PackageRuleConfig>();
        }

        /// <summary>
        /// 创建配置
        /// </summary>
        public static void CreateConfigAsset<T>() where T : ScriptableObject
        {
            string typeName = typeof(T).Name;
            string[] paths = AssetDatabase.FindAssets("t:" + typeName);
            if (paths.Length >= 1)
            {
                string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                EditorUtility.DisplayDialog("警告", $"已存在{typeName}，路径:{path}", "确认");
                return;
            }

            T config = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(config, $"Assets/{typeName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("提示", $"{typeName}创建完毕，路径:Assets/{typeName}.asset", "确认");
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
                throw new Exception("不存在" + typeName);
            }
            if (paths.Length > 1)
            {
                throw new Exception(typeName + "数量大于1");

            }
            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            T config = AssetDatabase.LoadAssetAtPath<T>(path);

            return config;
        }

        

       


       
    }

}
