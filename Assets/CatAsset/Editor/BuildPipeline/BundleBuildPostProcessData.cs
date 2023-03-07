using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建后处理数据
    /// </summary>
    public class BundleBuildPostProcessData
    {
        /// <summary>
        /// 构建配置
        /// </summary>
        public BundleBuildConfigSO Config;
        
        /// <summary>
        /// 目标平台
        /// </summary>
        public BuildTarget TargetPlatform;
        
        /// <summary>
        /// 输出文件夹
        /// </summary>
        public string OutputFolder;
        
        /// <summary>
        /// 返回码
        /// </summary>
        public ReturnCode ReturnCode;
        
        /// <summary>
        /// 资源包构建结果
        /// </summary>
        public IBundleBuildResults Result;
    }
}