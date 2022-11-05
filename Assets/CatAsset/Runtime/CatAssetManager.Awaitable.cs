using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Task = UnityEditor.VersionControl.Task;

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
    }
    
}