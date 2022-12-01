using UnityEditor;

namespace CatAsset.Editor
{
    
    public interface IPreprocessBuildBundles
    {
        /// <summary>
        /// 构建资源包前调用
        /// </summary>
        void OnPreprocessBuildBundles(BundleBuildConfigSO bundleBuildConfig, BuildTarget targetPlatform);
    }
}