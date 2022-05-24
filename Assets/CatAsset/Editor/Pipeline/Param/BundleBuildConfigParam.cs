using UnityEditor;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建配置参数
    /// </summary>
    public class BundleBuildConfigParam : IBuildPipelineParam
    {
        public BundleBuildConfigSO Config;
        public BuildTarget TargetPlatform;
    }
}