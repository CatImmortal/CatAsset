using UnityEditor;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建配置参数
    /// </summary>
    public interface IBundleBuildConfigParam : IContextObject
    {
        /// <summary>
        /// 资源包构建配置
        /// </summary>
        public BundleBuildConfigSO Config { get; }
        
        /// <summary>
        /// 目标平台
        /// </summary>
        public BuildTarget TargetPlatform { get; }
    }
}