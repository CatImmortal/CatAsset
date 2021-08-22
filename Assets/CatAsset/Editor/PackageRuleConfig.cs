using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CatAsset.Editor
{
    /// <summary>
    /// 打包规则配置
    /// </summary>
    public class PackageRuleConfig : ScriptableObject
    {
        [SerializeField]
        public List<PackageRule> Rules;

        [MenuItem("CatAsset/创建打包规则配置文件")]
        private static void CreateConfig()
        {
            string[] paths = AssetDatabase.FindAssets("t:PackageRuleConfig");
            if (paths.Length >= 1)
            {
                string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                EditorUtility.DisplayDialog("警告", $"已存在PackageRuleConfig，路径:{path}", "确认");
                return;
            }

            PackageRuleConfig setting = CreateInstance<PackageRuleConfig>();
            AssetDatabase.CreateAsset(setting, "Assets/PackageRuleConfig.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("提示", $"PackageRuleConfig创建完毕，路径:Assets/PackageRuleConfig.asset", "确认");
        }
    }
}

