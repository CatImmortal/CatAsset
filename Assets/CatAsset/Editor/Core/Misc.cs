using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace CatAsset.Editor
{

    public static class Misc
    {

        [MenuItem("CatAsset/打开目录/打包输出根目录", priority = 2)]
        private static void OpenAssetBundleOutputPath()
        {
            Open(PkgUtil.PkgCfg.OutputPath);
        }

        [MenuItem("CatAsset/打开目录/只读区", priority = 2)]
        private static void OpenReadOnlyPath()
        {
            Open(Application.streamingAssetsPath);
        }

        [MenuItem("CatAsset/打开目录/读写区", priority = 2)]
        private static void OpenReadWritePath()
        {
            Open(Application.persistentDataPath);
        }

        /// <summary>
        /// 打开指定目录
        /// </summary>
        private static void Open(string directory)
        {
            directory = string.Format("\"{0}\"", directory);

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                Process.Start("Explorer.exe", directory.Replace('/', '\\'));
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                Process.Start("open", directory);
            }
        }

        [MenuItem("Assets/复制资源路径",false)]
        private static void CopyAssetPath()
        {
            string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            GUIUtility.systemCopyBuffer = path;
        }

        [MenuItem("Assets/复制资源路径",true)]
        private static bool CopyAssetPathValidate()
        {
            if (Selection.assetGUIDs.Length == 0 || Selection.assetGUIDs.Length > 1)
            {
                //不允许多选
                return false;
            }

            string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            return File.Exists(path);
        }


        [MenuItem("Assets/添加为打包规则目录（可多选）", false)]
        private static void AddToPackageRuleDir()
        {
            foreach (string guid in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(path))
                {
                    PackageRule rule = new PackageRule();
                    rule.Directory = path;
                    PkgUtil.PkgRuleCfg.Rules.Add(rule);
                }
            }

            PkgUtil.PkgRuleCfg.Rules.Sort();
        }

        [MenuItem("Assets/添加为打包规则目录（可多选）", true)]
        private static bool AddToPackageRuleDirValidate()
        {
            foreach (string guid in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(path))
                {
                    return true;
                }
            }

            return false;
        }


    }

}

