using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 可等待扩展
    /// </summary>
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
        public static Task<ValueTuple<T,LoadAssetResult>> LoadAssetAsync<T>(string assetName,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<ValueTuple<T,LoadAssetResult>> tcs = new TaskCompletionSource<ValueTuple<T,LoadAssetResult>>();
            LoadAssetAsync<T>(assetName, (asset,result) =>
            {
                tcs.SetResult((asset,result));
               
            }, priority);
            return tcs.Task;
        }

        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public static Task<List<LoadAssetResult>> BatchLoadAssetAsync(List<string> assetNames,TaskPriority priority = TaskPriority.Low)
        {
            TaskCompletionSource<List<LoadAssetResult>> tcs = new TaskCompletionSource<List<LoadAssetResult>>();
            BatchLoadAssetAsync(assetNames, (assets) =>
            {
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