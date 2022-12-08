using System;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;
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
        /// 过滤器
        /// </summary>
        public string Filter;
        
        /// <summary>
        /// 正则表达式
        /// </summary>
        public string Regex;
        
        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

        /// <summary>
        /// 资源包压缩设置
        /// </summary>
        [SerializeField]
        internal BundleCompressOptions CompressOption;
        
        /// <summary>
        /// 目录名
        /// </summary>
        public string DirectoryName => AssetDatabase.GetAssetPath(DirectoryObj);

        public BundleBuildDirectory(string directoryName, string buildRuleName = nameof(NAssetToOneBundle),
            string regex = null, string group = GroupInfo.DefaultGroup,
            BundleCompressOptions compressOption = BundleCompressOptions.UseGlobal)
        {
            DirectoryObj = AssetDatabase.LoadAssetAtPath<Object>(directoryName);
            BuildRuleName = buildRuleName;
            Regex = regex;
            Group = group;
            CompressOption = compressOption;
        }

        public int CompareTo(BundleBuildDirectory other)
        {
            return DirectoryName.CompareTo(other.DirectoryName);
        }

        /// <summary>
        /// 获取压缩设置
        /// </summary>
        public BundleCompressOptions GetCompressOption()
        {
            BundleCompressOptions result = CompressOption;
            if (result == BundleCompressOptions.UseGlobal)
            {
                result = BundleBuildConfigSO.Instance.GlobalCompress;
            }
            return result;
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
                    BundleBuildDirectory directory = new BundleBuildDirectory(path);
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