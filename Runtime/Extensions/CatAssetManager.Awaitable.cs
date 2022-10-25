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
        public static Task<T> LoadAssetAsync<T>(string assetName,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            LoadAssetAsync<T>(assetName, (asset,result) =>
            {
                if (asset != null && target != null)
                {
                    BindToGameObject(target, result.GetAsset());
                }

                tcs.SetResult(result.GetAsset<T>());
               
            }, priority);
            return tcs.Task;
        }
        
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<T> LoadAssetAsync<T>(string assetName,Scene target = default,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            LoadAssetAsync(assetName, (asset, result) =>
            {
                if (asset != null && target != default)
                {
                    BindToScene(target,result.GetAsset());
                }
                tcs.SetResult(result.GetAsset<T>());
               
            }, priority);
            return tcs.Task;
        }

        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public static Task<List<LoadAssetResult>> BatchLoadAssetAsync(List<string> assetNames,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<List<LoadAssetResult>> tcs = new TaskCompletionSource<List<LoadAssetResult>>();
            BatchLoadAssetAsync(assetNames, (assets) =>
            {
                if (target != null)
                {
                    foreach (LoadAssetResult result in assets)
                    {
                        object asset = result.GetAsset();
                        if (asset != null)
                        {
                            BindToGameObject(target,asset);
                        }
                    }
                }
                
                tcs.SetResult(assets);
               
            }, priority);
            
            return tcs.Task;
        }
        
        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public static Task<List<LoadAssetResult>> BatchLoadAssetAsync(List<string> assetNames,Scene target = default,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<List<LoadAssetResult>> tcs = new TaskCompletionSource<List<LoadAssetResult>>();
            BatchLoadAssetAsync(assetNames, (assets) =>
            {
                if (target != default)
                {
                    foreach (LoadAssetResult result in assets)
                    {
                        object asset = result.GetAsset();
                        if (asset != null)
                        {
                            BindToScene(target, asset);
                        }
                    }
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