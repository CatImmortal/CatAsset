using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CatAsset.Editor
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// 要排除的文件后缀名集合
        /// </summary>
        public static readonly HashSet<string> ExcludeSet = new HashSet<string>();

        /// <summary>
        /// 默认资源组
        /// </summary>
        public const string DefaultGroup = "Base";
        

        static Util()
        {
            ExcludeSet.Add(".meta");
            ExcludeSet.Add(".cs");
            ExcludeSet.Add(".asmdef");
            ExcludeSet.Add(".giparams");
        }

        [MenuItem("CatAsset/打开目录/资源包构建输出根目录", priority = 2)]
        private static void OpenAssetBundleOutputPath()
        {
            Open(GetConfigAsset<BundleBuildConfigSO>().OutputPath);
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

        [MenuItem("Assets/添加为资源包构建目录（可多选）", false)]
        private static void AddToBundleBuildDirectory()
        {
            BundleBuildConfigSO config = GetConfigAsset<BundleBuildConfigSO>();

            foreach (string guid in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(path) && config.CanAddDirectory(path))
                {
                    BundleBuildDirectory directory = new BundleBuildDirectory(path,nameof(NAssetToOneBundle),DefaultGroup);
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
                if (Directory.Exists(path))
                {
                    return true;
                }
            }

            return false;
        }
        
        
        /// <summary>
        /// 获取SO配置
        /// </summary>
        public static T GetConfigAsset<T>() where T : ScriptableObject
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
        /// 将完整目录/文件名转换为Assets开头的目录/文件名
        /// </summary>
        public static string FullNameToAssetName(string fullName)
        {
            int assetsIndex = fullName.IndexOf("Assets\\");
            string assetsDir = fullName.Substring(assetsIndex).Replace('\\', '/');
            return assetsDir;
        }
        
        /// <summary>
        /// 获取排除了自身和csharp代码文件的依赖资源列表
        /// </summary>
        public static List<string> GetDependencies(string assetName,bool recursive = true)
        {
            List<string> result  = null;
            
            string[] dependencies = AssetDatabase.GetDependencies(assetName,recursive);

            if (dependencies.Length == 0)
            {
                return result;
            }

            result = new List<string>();
        
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependencyName = dependencies[i];
                if (dependencyName == assetName || dependencyName.EndsWith(".cs"))
                {
                    continue;
                }

                result.Add(dependencyName);
            }

            return result;
        }

        /// <summary>
        /// 获取完整资源包构建输出目录
        /// </summary>
        public static string GetFullOutputPath(string outputPath, BuildTarget targetPlatform, int manifestVersion)
        {
            string dir = Application.version + "_" + manifestVersion;
            string result = Path.Combine(outputPath, targetPlatform.ToString(), dir);
            return result;
        }
        
        /// <summary>
        /// 创建空目录（若存在则清空）
        /// </summary>
        public static void CreateEmptyDirectory(string directory)
        {
            //目录已存在就删除
            if (Directory.Exists(directory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                DeleteDirectory(dirInfo);
            }
            
            //创建目录
            Directory.CreateDirectory(directory);
        }
        
        /// <summary>
        /// 删除指定目录
        /// </summary>
        public static void DeleteDirectory(DirectoryInfo dirInfo)
        {
            //删除当前目录下的所有文件
            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                fileInfo.Delete();
            }

            //递归删除子目录
            foreach (DirectoryInfo childDirInfo in dirInfo.GetDirectories())
            {
                DeleteDirectory(childDirInfo);
            }
            
            //删除自身
            dirInfo.Delete();
        }

        /// <summary>
        /// 将原生资源清单合并至主资源清单
        /// </summary>
        public static void MergeManifest(CatAssetManifest main, CatAssetManifest raw)
        {
            for (int i = main.Bundles.Count - 1; i >= 0; i--)
            {
                if (main.Bundles[i].IsRaw)
                {
                    main.Bundles.RemoveAt(i);
                }
            }

            foreach (BundleManifestInfo bundleManifestInfo in raw.Bundles)
            {
                main.Bundles.Add(bundleManifestInfo);
            }
        }
    }
}