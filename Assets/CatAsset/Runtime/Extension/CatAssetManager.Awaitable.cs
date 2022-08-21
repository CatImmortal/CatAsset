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
        public static Task<object> AwaitLoadAsset(string assetName,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            LoadAsset(assetName, null, (success, asset, userdata) =>
            {
                if (success && target != null)
                {
                    if (asset is Object unityAsset)
                    {
                        BindToGameObject(target, unityAsset);
                    }
                    else if (asset is byte[] rawAsset)
                    {
                        BindToGameObject(target, rawAsset);
                    }
                }

                tcs.SetResult(asset);
               
            }, priority);
            return tcs.Task;
        }
        
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<object> AwaitLoadAsset(string assetName,Scene target = default,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            LoadAsset(assetName, null, (success, asset, userdata) =>
            {
                if (success && target != default)
                {
                    BindToScene(target,asset);
                }
                tcs.SetResult(asset);
               
            }, priority);
            return tcs.Task;
        }

        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public static Task<List<object>> AwaitBatchLoadAsset(List<string> assetNames,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<List<object>> tcs = new TaskCompletionSource<List<object>>();
            BatchLoadAsset(assetNames, null, (assets, userdata) =>
            {
                if (target != null)
                {
                    foreach (object asset in assets)
                    {
                        if (asset != null)
                        {
                            if (asset is Object unityObj)
                            {
                                BindToGameObject(target,unityObj);
                            }
                            else
                            {
                                BindToGameObject(target,(byte[])asset);
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
        public static Task<List<object>> AwaitBatchLoadAsset(List<string> assetNames,Scene target = default,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<List<object>> tcs = new TaskCompletionSource<List<object>>();
            BatchLoadAsset(assetNames, null, (assets, userdata) =>
            {
                if (target != default)
                {
                    foreach (object asset in assets)
                    {
                        if (asset != null)
                        {
                            if (asset is Object unityObj)
                            {
                                BindToScene(target,unityObj);
                            }
                            else
                            {
                                BindToScene(target,(byte[])asset);
                            }
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
                ((TaskCompletionSource<Scene>)userdata).SetResult(scene);
            });
            
            return tcs.Task;
        }
    }
    
}