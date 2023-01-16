using UnityEditor;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建前的处理回调接口
    /// </summary>
    public interface IBundleBuildPreProcessor
    {
        /// <summary>
        /// 构建资源包前调用
        /// </summary>
        void OnBundleBuildPreProcess(BundleBuildConfigSO bundleBuildConfig, BuildTarget targetPlatform);
    }
}