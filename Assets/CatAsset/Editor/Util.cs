using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace CatAsset.Editor
{
    public static class Util
    {
        public static PackageRuleConfig GetPackageRuleConfig()
        {
            string[] paths = AssetDatabase.FindAssets("t:PackageRuleConfig");
            if (paths.Length == 0)
            {
                throw new System.Exception("不存在PackageRuleConfig，请点击CatAsset/创建打包配置进行创建");
            }
            if (paths.Length > 1)
            {
                throw new Exception("PackageRuleConfig数量大于1");

            }
            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            PackageRuleConfig config = AssetDatabase.LoadAssetAtPath<PackageRuleConfig>(path);

            return config;
        }
    }

}
