using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建后的后处理回调接口
    /// </summary>
    public interface IBundleBuildPostProcessor
    {
        /// <summary>
        /// 构建资源包后调用
        /// </summary>
        void OnBundleBuildPostProcess(BundleBuildPostProcessData data);
    }
}