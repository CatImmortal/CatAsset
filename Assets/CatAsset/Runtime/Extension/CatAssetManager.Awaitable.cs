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
        public static Task<T> AwaitLoadAsset<T>(string assetName,GameObject target = null,TaskPriority priority = TaskPriority.Middle) where T : Object
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            LoadAsset<T>(assetName, null, (success, asset, userdata) =>
            {
                if (success && target != null)
                {
                    BindToGameObject(target,asset);
                }
                tcs.SetResult(asset);
               
            }, priority);
            return tcs.Task;
        }
        
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<T> AwaitLoadAsset<T>(string assetName,Scene target = default,TaskPriority priority = TaskPriority.Middle) where T : Object
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            LoadAsset<T>(assetName, null, (success, asset, userdata) =>
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
        /// 加载原生资源（可等待）
        /// </summary>
        public static Task<byte[]> AwaitLoadRawAsset(string assetName,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
            LoadRawAsset(assetName, null, (success, asset, userdata) =>
            {
                if (success && target != null)
                {
                    BindToGameObject(target,asset);
                }
                tcs.SetResult(asset);
               
            }, priority);
            return tcs.Task;
        }
        
        /// <summary>
        /// 加载原生资源（可等待）
        /// </summary>
        public static Task<byte[]> AwaitLoadRawAsset(string assetName,Scene target = default,TaskPriority priority = TaskPriority.Middle)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
            LoadRawAsset(assetName, null, (success, asset, userdata) =>
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