using UnityEditor;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建预处理数据
    /// </summary>
    public class BundleBuildPreProcessData
    {
        /// <summary>
        /// 构建配置
        /// </summary>
        public BundleBuildConfigSO Config;
        
        /// <summary>
        /// 目标平台
        /// </summary>
        public BuildTarget TargetPlatform;
    }
}