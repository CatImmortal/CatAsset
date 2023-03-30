using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

namespace CatAsset.Editor
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static class EditorUtil
    {
        /// <summary>
        /// 构建补丁包时产生的临时依赖冗余包名
        /// </summary>
        public const string RedundancyDepBundleName = "RedundancyDepBundle.bundle";
        
        /// <summary>
        /// 要排除的文件后缀名集合
        /// </summary>
        public static readonly HashSet<string> ExcludeSet = new HashSet<string>()
        {
            ".meta",
            ".cs",
            ".asmdef",
            ".asmref",
            ".giparams",
            ".so",
            ".dll",
            ".cginc",
        };

        /// <summary>
        /// 无依赖的资源类型
        /// </summary>
        public static readonly HashSet<Type> NotDependencyAssetType = new HashSet<Type>()
        {
            typeof(Texture2D),
            typeof(Texture3D),
            typeof(Shader),
            typeof(TextAsset),
            typeof(AudioClip),
            typeof(VideoClip),
            typeof(DefaultAsset),
        };

        [MenuItem("CatAsset/打开目录/资源包构建输出根目录", priority = 3)]
        private static void OpenAssetBundleOutputPath()
        {
            OpenDirectory(BundleBuildConfigSO.Instance.OutputRootDirectory);
        }
        

        [MenuItem("CatAsset/打开目录/只读区", priority = 3)]
        private static void OpenReadOnlyPath()
        {
            OpenDirectory(Application.streamingAssetsPath);
        }

        [MenuItem("CatAsset/打开目录/读写区", priority = 3)]
        private static void OpenReadWritePath()
        {
            OpenDirectory(Application.persistentDataPath);
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
            List<string> result = new List<string>();
            
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
            if (AssetDatabase.IsValidFolder(assetName))
            {
                //文件夹
                return false;
            }
            
            string fileExtension = Path.GetExtension(assetName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                //无后缀的
                return false;
            }

            if (ExcludeSet.Contains(fileExtension))
            {
                //被排除的
                return false;
            }

            Type type = AssetDatabase.GetMainAssetTypeAtPath(assetName);
            if (type == typeof(LightingDataAsset))
            {
                //光照
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
            string assetsDir = RuntimeUtil.GetRegularPath(fullName.Substring(assetsIndex));
            return assetsDir;
        }

        /// <summary>
        /// 获取完整资源包构建输出目录
        /// </summary>
        public static string GetFullOutputFolder(string outputDir, BuildTarget targetPlatform, int manifestVersion)
        {
            string dir = manifestVersion.ToString();
            string result = Path.Combine(outputDir, targetPlatform.ToString(), dir);
            return result;
        }
        
        /// <summary>
        /// 获取资源包缓存目录
        /// </summary>
        public static string GetBundleCacheFolder(string outputDir, BuildTarget targetPlatform)
        {
            string result = Path.Combine(outputDir, targetPlatform.ToString(),"Cache");
            result = RuntimeUtil.GetRegularPath(result);
            return result;
        }

        /// <summary>
        /// 获取缓存的资源清单路径
        /// </summary>
        public static string GetCacheManifestPath(string outputDir, BuildTarget targetPlatform)
        {
            string bundleCacheFolder = GetBundleCacheFolder(outputDir,targetPlatform);
            string manifestPath = RuntimeUtil.GetRegularPath(Path.Combine(bundleCacheFolder, CatAssetManifest.ManifestJsonFileName));
            return manifestPath;
        }

        /// <summary>
        /// 获取资源缓存清单目录
        /// </summary>
        public static string GetAssetCacheManifestFolder(string outputDir)
        {
            string result = Path.Combine(outputDir,"Cache");
            result = RuntimeUtil.GetRegularPath(result);
            return result;
        }

        /// <summary>
        /// 获取资源缓存清单路径
        /// </summary>
        public static string GetAssetCacheManifestPath(string outputDir)
        {
            string assetCacheManifestFolder = EditorUtil.GetAssetCacheManifestFolder(outputDir);
            string assetCacheManifestPath = RuntimeUtil.GetRegularPath(Path.Combine(assetCacheManifestFolder, AssetCacheManifest.ManifestJsonFileName));
            return assetCacheManifestPath;
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
        /// 打开指定目录
        /// </summary>
        public static void OpenDirectory(string directory)
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
        /// 复制文件夹
        /// </summary>
        public static void CopyDirectory(string sourceDir, string destDir,bool skipExistsDestFile = true)
        {
            if (!Directory.Exists(destDir)) // 如果目标文件夹不存在，则创建一个
            {
                Directory.CreateDirectory(destDir);
            }
            string[] files = Directory.GetFiles(sourceDir); // 获取源文件夹中的所有文件
            foreach (string file in files) // 遍历所有文件，并将其复制到目标文件夹中
            {
                
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                if (skipExistsDestFile && File.Exists(destFile))
                {
                    //跳过已存在的目标文件
                    continue;
                }
                File.Copy(file, destFile, true);
            }
            string[] dirs = Directory.GetDirectories(sourceDir); // 获取源文件夹中的所有子文件夹
            foreach (string dir in dirs) // 递归遍历所有子文件夹，并将其复制到目标文件夹中
            {
                string dirName = Path.GetFileName(dir);
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(dir, destSubDir,skipExistsDestFile);
            }
        }
        
        /// <summary>
        /// 重命名文件
        /// </summary>
        public static void RenameFile(string filePath, string newFileName)
        {
            string directory = Path.GetDirectoryName(filePath);
            string newFilePath = Path.Combine(directory, newFileName);
            File.Move(filePath, newFilePath);
        }

        /// <summary>
        /// 获取指定时间的格式字符串
        /// </summary>
        public static string GetDateTimeStr(DateTime dateTime)
        {
            string year = dateTime.Year.ToString();
            string month = dateTime.Month.ToString();
            string day = dateTime.Day.ToString();

            string hour = dateTime.Hour.ToString();
            string minute = dateTime.Minute.ToString();
            string second = dateTime.Second.ToString();
            
            string dateTimeStr = $"{year}.{month}.{day}-{hour}.{minute}.{second}";
            return dateTimeStr;
        }
    }
}