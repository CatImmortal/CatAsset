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
            Util.CreateConfigAsset<PackageRuleConfig>();
        }
    }
}

