using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建目录
    /// </summary>
    [Serializable]
    public class BundleBuildDirectory : IComparable<BundleBuildDirectory>
    {
        /// <summary>
        /// 目录对象
        /// </summary>
        public Object DirectoryObj;
        
        /// <summary>
        /// 构建规则名
        /// </summary>
        public string BuildRuleName;

        /// <summary>
        /// 构建规则所使用的正则表达式
        /// </summary>
        public string RuleRegex;
        
        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

        /// <summary>
        /// 目录名
        /// </summary>
        public string DirectoryName => AssetDatabase.GetAssetPath(DirectoryObj);

        public BundleBuildDirectory(string directoryName, string buildRuleName, string ruleRegex, string group)
        {
            DirectoryObj = AssetDatabase.LoadAssetAtPath<Object>(directoryName);
            BuildRuleName = buildRuleName;
            RuleRegex = ruleRegex;
            Group = group;
        }

        public int CompareTo(BundleBuildDirectory other)
        {
            //return DirectoryName.CompareTo(other.DirectoryName);
            return DirectoryName.CompareTo(other.DirectoryName);
        }
    }
}