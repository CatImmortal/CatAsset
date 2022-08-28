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
        public static Task<bool> AwaitCheckPackageManifest()
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
        public static Task<T> AwaitLoadAsset<T>(string assetName,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            LoadAsset<T>(assetName, null, (success, asset,result, userdata) =>
            {
                if (success && target != null)
                {
                    if (result.Category == AssetCategory.InternalBundleAsset)
                    {
                        BindToGameObject(target,result.GetAsset<Object>());
                    }
                    else
                    {
                        BindToGameObject(target,result.GetAsset<byte[]>());
                    }
                }

                tcs.SetResult(result.GetAsset<T>());
               
            }, priority);
            return tcs.Task;
        }
        
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<T> AwaitLoadAsset<T>(string assetName,Scene target = default,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            LoadAsset(assetName, null, (success,asset, result, userdata) =>
            {
                if (success && target != default)
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
        public static Task<List<LoadAssetResult>> AwaitBatchLoadAsset(List<string> assetNames,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<List<LoadAssetResult>> tcs = new TaskCompletionSource<List<LoadAssetResult>>();
            BatchLoadAsset(assetNames, null, (assets, userdata) =>
            {
                if (target != null)
                {
                    foreach (LoadAssetResult result in assets)
                    {
                        object asset = result.GetAsset();
                        if (asset != null)
                        {
                            if (result.Category == AssetCategory.InternalBundleAsset)
                            {
                                BindToGameObject(target,result.GetAsset<Object>());
                            }
                            else
                            {
                                BindToGameObject(target,result.GetAsset<byte[]>());
                            }
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
        public static Task<List<LoadAssetResult>> AwaitBatchLoadAsset(List<string> assetNames,Scene target = default,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<List<LoadAssetResult>> tcs = new TaskCompletionSource<List<LoadAssetResult>>();
            BatchLoadAsset(assetNames, null, (assets, userdata) =>
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
        public static Task<Scene> AwaitLoadScene(string sceneName)
        {
            TaskCompletionSource<Scene> tcs = new TaskCompletionSource<Scene>();

            LoadScene(sceneName, tcs, (success, scene, userdata) =>
            {
                TaskCompletionSource<Scene> localTcs = (TaskCompletionSource<Scene>)userdata;
                localTcs.SetResult(scene);
            });
            
            return tcs.Task;
        }
    }
    
}