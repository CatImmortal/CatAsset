using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CatAsset.Editor
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static class EditorUtil
    {
        /// <summary>
        /// 要排除的文件后缀名集合
        /// </summary>
        public static readonly HashSet<string> ExcludeSet = new HashSet<string>()
        {
            ".meta",
            ".cs",
            ".asmdef",
            ".giparams",
            ".so",
            ".dll",
            ".cginc",
        };

        /// <summary>
        /// 默认资源组
        /// </summary>
        public const string DefaultGroup = "Base";

        [MenuItem("CatAsset/打开目录/资源包构建输出根目录", priority = 3)]
        private static void OpenAssetBundleOutputPath()
        {
            Open(BundleBuildConfigSO.Instance.OutputPath);
        }
        

        [MenuItem("CatAsset/打开目录/只读区", priority = 3)]
        private static void OpenReadOnlyPath()
        {
            Open(Application.streamingAssetsPath);
        }

        [MenuItem("CatAsset/打开目录/读写区", priority = 3)]
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
            BundleBuildConfigSO config = BundleBuildConfigSO.Instance;

            foreach (string guid in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(path) && config.CanAddDirectory(path))
                {
                    BundleBuildDirectory directory = new BundleBuildDirectory(path,nameof(NAssetToOneBundle),null,DefaultGroup);
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
        
        
        [MenuItem("Assets/刷新资源包构建信息")]
        private static void RefreshBundleBuildInfo()
        {
            if (BundleBuildConfigSO.Instance != null)
            {
                BundleBuildConfigSO.Instance.RefreshBundleBuildInfos();
            }
        }

        /// <summary>
        /// 获取实现了指定类型的所有类的对象
        /// </summary>
        public static List<T> GetAssignableTypeObjects<T>()
        {
            List<T> objList = new List<T>();
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<T>();
            foreach (Type type in types)
            {
                T obj = (T) Activator.CreateInstance(type);
                objList.Add(obj);
            }

            return objList;
        }

        /// <summary>
        /// 调用私有的静态方法
        /// </summary>
        public static object InvokeNonPublicStaticMethod(Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
            {
                Debug.LogError($"类型：{type.FullName} 中未找到方法: {method}");
                return null;
            }
            return methodInfo.Invoke(null, parameters);
        }
        
        
        /// <summary>
        /// 获取排除了自身和无效文件的依赖资源列表
        /// </summary>
        public static List<string> GetDependencies(string assetName,bool recursive = true)
        {
            List<string> result  = new List<string>();
            
            string[] dependencies = AssetDatabase.GetDependencies(assetName,recursive);

            if (dependencies.Length == 0)
            {
                return result;
            }

            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependencyName = dependencies[i];
                if (!IsValidAsset(dependencyName))
                {
                    continue;
                }
                if (dependencyName == assetName)
                {
                    continue;
                }

                result.Add(dependencyName);
            }

            return result;
        }

        /// <summary>
        /// 是否为有效资源
        /// </summary>
        public static bool IsValidAsset(string assetName)
        {
            string fileExtension = Path.GetExtension(assetName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                return false;
            }

            if (ExcludeSet.Contains(fileExtension))
            {
                return false;
            }

            Type type = AssetDatabase.GetMainAssetTypeAtPath(assetName);
            if (type == typeof(LightingDataAsset))
            {
                return false;
            }

            return true;

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
        


    }
}