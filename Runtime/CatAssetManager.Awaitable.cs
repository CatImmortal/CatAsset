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
        /// 加载资源（可等待）
        /// </summary>
        public static Task<T> LoadAssetAsync<T>(string assetName,GameObject target,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            LoadAssetAsync<T>(assetName, (asset,result) =>
            {
                if (target != null)
                {
                    target.Bind(result.Asset);
                }
                tcs.SetResult(asset);
               
            }, priority);
            return tcs.Task;
        }
        
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<T> LoadAssetAsync<T>(string assetName,Scene scene,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            LoadAssetAsync<T>(assetName, (asset,result) =>
            {
                if (scene != default)
                {
                    scene.Bind(result.Asset);
                }
                tcs.SetResult(asset);
               
            }, priority);
            return tcs.Task;
        }

        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public static Task<List<LoadAssetResult>> BatchLoadAssetAsync(List<string> assetNames,GameObject target,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<List<LoadAssetResult>> tcs = new TaskCompletionSource<List<LoadAssetResult>>();
            BatchLoadAssetAsync(assetNames, (assets) =>
            {
                foreach (LoadAssetResult result in assets)
                {
                    target.Bind(result.Asset);
                }
                tcs.SetResult(assets);
               
            }, priority);
            
            return tcs.Task;
        }
        
        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public static Task<List<LoadAssetResult>> BatchLoadAssetAsync(List<string> assetNames,Scene target,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<List<LoadAssetResult>> tcs = new TaskCompletionSource<List<LoadAssetResult>>();
            BatchLoadAssetAsync(assetNames, (assets) =>
            {
                foreach (LoadAssetResult result in assets)
                {
                    target.Bind(result.Asset);
                }
                tcs.SetResult(assets);
               
            }, priority);
            
            return tcs.Task;
        }
        

        /// <summary>
        /// 加载场景(可等待)
        /// </summary>
        public static Task<Scene> LoadSceneAsync(string sceneName)
        {
            TaskCompletionSource<Scene> tcs = new TaskCompletionSource<Scene>();

            LoadSceneAsync(sceneName, (success, scene) =>
            {
                tcs.SetResult(scene);
            });
            
            return tcs.Task;
        }
    }
    
}