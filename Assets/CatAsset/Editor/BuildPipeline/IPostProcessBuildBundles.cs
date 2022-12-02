using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    public interface IPostProcessBuildBundles
    {
        /// <summary>
        /// 构建资源包后调用
        /// </summary>
        void OnPostProcessBuildBundles(BundleBuildConfigSO bundleBuildConfig, BuildTarget targetPlatform,string outputFolder,
            ReturnCode returnCode, IBundleBuildResults result);
    }
}