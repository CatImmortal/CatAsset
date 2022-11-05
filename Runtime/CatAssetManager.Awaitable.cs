using System.Threading.Tasks;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
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
    }

}
