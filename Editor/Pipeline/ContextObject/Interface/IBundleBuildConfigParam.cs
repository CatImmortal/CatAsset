using UnityEditor;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建配置参数
    /// </summary>
    public interface IBundleBuildConfigParam : IContextObject
    {
        public BundleBuildConfigSO Config { get; }
        public BuildTarget TargetPlatform { get; }
    }
}