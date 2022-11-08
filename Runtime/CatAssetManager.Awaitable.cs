﻿using UnityEngine;

#if !UNITASK
using System.Threading.Tasks;
#else
using Cysharp.Threading.Tasks;
#endif


namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
#if !UNITASK
        /// <summary>
        /// 检查安装包内资源清单,单机模式下专用（可等待）
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
        /// 检查资源版本，可更新模式下专用（可等待）
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
        /// 加载预制体并实例化，会自行将Handler绑定至实例化出的游戏物体上（可等待）
        /// </summary>
        public static Task<GameObject> InstantiateAsync(string prefabName,Transform parent = null)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
            InstantiateAsync(prefabName, (go =>
            {
                tcs.SetResult(go);
            }), parent);
            return tcs.Task;
        }
#else
        /// <summary>
        /// 检查安装包内资源清单,单机模式下专用（可等待）
        /// </summary>
        public static UniTask<bool> CheckPackageManifest()
        {
            UniTaskCompletionSource<bool> tcs = new UniTaskCompletionSource<bool>();
            CheckPackageManifest(success =>
            {
                tcs.TrySetResult(success);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 检查资源版本，可更新模式下专用（可等待）
        /// </summary>
        public static UniTask<VersionCheckResult> CheckVersion()
        {
            UniTaskCompletionSource<VersionCheckResult> tcs = new UniTaskCompletionSource<VersionCheckResult>();
            CheckVersion((result =>
            {
                tcs.TrySetResult(result);
            }));
            return tcs.Task;
        }
        
         /// <summary>
        /// 加载预制体并实例化，会自行将Handler绑定至实例化出的游戏物体上（可等待）
        /// </summary>
        public static UniTask<GameObject> InstantiateAsync(string prefabName,Transform parent = null)
        {
            UniTaskCompletionSource<GameObject> tcs = new UniTaskCompletionSource<GameObject>();
            InstantiateAsync(prefabName, (go =>
            {
                tcs.TrySetResult(go);
            }), parent);
            return tcs.Task;
        }
#endif
    }

}
