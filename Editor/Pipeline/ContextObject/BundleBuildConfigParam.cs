using UnityEditor;

namespace CatAsset.Editor
{
    /// <inheritdoc />
    public class BundleBuildConfigParam : IBundleBuildConfigParam
    {
        /// <inheritdoc />
        public BundleBuildConfigSO Config { get; }
        
        /// <inheritdoc />
        public BuildTarget TargetPlatform { get; }

        public BundleBuildConfigParam(BundleBuildConfigSO config,BuildTarget targetPlatform)
        {
            Config = config;
            TargetPlatform = targetPlatform;
        }
    }
}