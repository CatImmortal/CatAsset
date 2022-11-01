using System;
using System.IO;
using UnityEngine;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 检查安装包内资源清单,仅使用安装包内资源模式下专用
        /// </summary>
        public static void CheckPackageManifest(Action<bool> callback)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                callback?.Invoke(true);
                return;
            }
#endif
            
            if (RuntimeMode != RuntimeMode.PackageOnly)
            {
                Debug.LogError("PackageOnly模式下才能调用CheckPackageManifest");
                callback(false);
                return;
            }

            string path = RuntimeUtil.GetReadOnlyPath(RuntimeUtil.ManifestFileName);

            WebRequestTask task = WebRequestTask.Create(loadTaskRunner, path, path, callback,
                (success, uwr, userdata) =>
                {
                    Action<bool> onChecked = (Action<bool>) userdata;

                    if (!success)
                    {
                        Debug.LogError($"单机模式资源清单检查失败:{uwr.error}");
                        onChecked?.Invoke(false);
                    }
                    else
                    {
                        CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(uwr.downloadHandler.text);
                        CatAssetDatabase.InitPackageManifest(manifest);

                        Debug.Log("单机模式资源清单检查完毕");
                        onChecked?.Invoke(true);
                    }
                });

            loadTaskRunner.AddTask(task, TaskPriority.VeryLow);
        }

        /// <summary>
        /// 检查资源版本，可更新资源模式下专用
        /// </summary>
        public static void CheckVersion(OnVersionChecked onVersionChecked)
        {
#if UNITY_EDITOR
            if (IsEditorMode)
            {
                onVersionChecked?.Invoke(new VersionCheckResult(null,0,0));
                return;
            }
#endif
            
            if (RuntimeMode != RuntimeMode.Updatable)
            {
                Debug.LogError("Updatable模式下才能调用CheckVersion");
                return;
            }

            VersionChecker.CheckVersion(onVersionChecked);
        }

        /// <summary>
        /// 检查可更新模式下指定路径的资源清单
        /// </summary>
        internal static void CheckUpdatableManifest(string path, WebRequestCallback callback)
        {
            WebRequestTask task = WebRequestTask.Create(downloadTaskRunner, path, path, null, callback);
            downloadTaskRunner.AddTask(task, TaskPriority.VeryLow);
        }

        /// <summary>
        /// 从外部导入资源清单
        /// </summary>
        public static void ImportInternalAsset(string manifestPath, Action<bool> callback,
            string bundleRelativePathPrefix = null)
        {
            manifestPath = RuntimeUtil.GetReadWritePath(manifestPath);
            WebRequestTask task = WebRequestTask.Create(loadTaskRunner, manifestPath, manifestPath, callback,
                (success, uwr, userdata) =>
                {
                    Action<bool> onChecked = (Action<bool>) userdata;

                    if (!success)
                    {
                        Debug.LogError($"内置资源导入失败:{uwr.error}");
                        onChecked?.Invoke(false);
                    }
                    else
                    {
                        CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(uwr.downloadHandler.text);

                        foreach (BundleManifestInfo bundleManifestInfo in manifest.Bundles)
                        {
                            if (!string.IsNullOrEmpty(bundleRelativePathPrefix))
                            {
                                //为资源包目录名添加额外前缀
                                bundleManifestInfo.Directory = RuntimeUtil.GetRegularPath(Path.Combine(bundleRelativePathPrefix,
                                    bundleManifestInfo.Directory));
                            }

                            CatAssetDatabase.InitRuntimeInfo(bundleManifestInfo, true);
                        }

                        Debug.Log("内置资源导入完毕");
                        onChecked?.Invoke(true);
                    }
                });

            loadTaskRunner.AddTask(task, TaskPriority.Height);
        }
    }
}