using System;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建目录
    /// </summary>
    [Serializable]
    public class BundleBuildDirectory : IComparable<BundleBuildDirectory>
    {
        /// <summary>
        /// 目录名
        /// </summary>
        public string DirectoryName;
        
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


        public BundleBuildDirectory(string directoryName, string buildRuleName, string ruleRegex, string group)
        {
            DirectoryName = directoryName;
            BuildRuleName = buildRuleName;
            RuleRegex = ruleRegex;
            Group = group;
        }

        public int CompareTo(BundleBuildDirectory other)
        {
            return DirectoryName.CompareTo(other.DirectoryName);
        }
    }
}