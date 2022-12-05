using System;
using System.IO;
using CatAsset.Runtime;
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
            return DirectoryName.CompareTo(other.DirectoryName);
        }
        
        [MenuItem("Assets/添加为资源包构建目录（可多选）", false)]
        private static void AddToBundleBuildDirectory()
        {
            BundleBuildConfigSO config = BundleBuildConfigSO.Instance;

            foreach (string guid in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (config.CanAddDirectory(path))
                {
                    BundleBuildDirectory directory = new BundleBuildDirectory(path,nameof(NAssetToOneBundle),null,GroupInfo.DefaultGroup);
                    config.Directories.Add(directory);
                }
            }
            config.Directories.Sort();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/添加为资源包构建目录（可多选）", true)]
        private static bool AddToBundleBuildDirectoryValidate()
        {
            foreach (string guid in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path))
                {
                    return true;
                }

                if (EditorUtil.IsValidAsset(path))
                {
                    return true;
                }
            }
        
            return false;
        }
    }
}