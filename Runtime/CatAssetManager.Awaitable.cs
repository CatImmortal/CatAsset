using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 检查安装包内资源清单,仅使用安装包内资源模式下专用（可等待）
        /// </summary>
        public static Task<bool> CheckPackageManifest()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CheckPackageManifest(success =>
            {
                tcs.SetResult(success);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 检查资源版本，可更新模式下专用
        /// </summary>
        public static Task<VersionCheckResult> CheckVersion()
        {
            TaskCompletionSource<VersionCheckResult> tcs = new TaskCompletionSource<VersionCheckResult>();
            CheckVersion((result =>
            {
                tcs.SetResult(result);
            }));
            return tcs.Task;
        }

        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<AssetHandler<T>> LoadAssetAsync<T>(string assetName,GameObject target,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<AssetHandler<T>> tcs = new TaskCompletionSource<AssetHandler<T>>();

            LoadAssetAsync<T>(assetName, priority).OnLoaded += handler =>
            {
                if (target != null)
                {
                    target.Bind(handler);
                }
                tcs.SetResult(handler);
            };
            
            return tcs.Task;

        }
        
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<AssetHandler<T>> LoadAssetAsync<T>(string assetName,Scene target,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<AssetHandler<T>> tcs = new TaskCompletionSource<AssetHandler<T>>();

            LoadAssetAsync<T>(assetName, priority).OnLoaded += handler =>
            {
                if (target != default)
                {
                    target.Bind(handler);
                }
                tcs.SetResult(handler);
            };
            
            return tcs.Task;

        }

        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public static Task<List<AssetHandler<object>>> BatchLoadAssetAsync(List<string> assetNames,GameObject target,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<List<AssetHandler<object>>> tcs = new TaskCompletionSource<List<AssetHandler<object>>>();

            BatchLoadAssetAsync(assetNames, priority).OnLoaded += handlers =>
            {
                if (target != null)
                {
                    foreach (AssetHandler<object> handler in handlers)
                    {
                        target.Bind(handler);
                    }
                }

                tcs.SetResult(handlers);
            };
            return tcs.Task;
            
           
        }
        
        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public static Task<List<AssetHandler<object>>> BatchLoadAssetAsync(List<string> assetNames,Scene target,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<List<AssetHandler<object>>> tcs = new TaskCompletionSource<List<AssetHandler<object>>>();

            BatchLoadAssetAsync(assetNames, priority).OnLoaded += handlers =>
            {
                if (target != default)
                {
                    foreach (AssetHandler<object> handler in handlers)
                    {
                        target.Bind(handler);
                    }
                }

                tcs.SetResult(handlers);
            };
            return tcs.Task;
            
           
        }

        /// <summary>
        /// 加载场景(可等待)
        /// </summary>
        public static Task<SceneHandler> AwaitLoadSceneAsync(string sceneName)
        {
            TaskCompletionSource<SceneHandler> tcs = new TaskCompletionSource<SceneHandler>();

            LoadSceneAsync(sceneName).OnLoaded += handler =>
            {
                tcs.SetResult(handler);
            };
            return tcs.Task;
        }
    }
    
}