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
        
        /// <inheritdoc />
        public bool IsBuildPatch { get; }

        public BundleBuildConfigParam(BundleBuildConfigSO config,BuildTarget targetPlatform,bool isBuildPatch)
        {
            Config = config;
            TargetPlatform = targetPlatform;
            IsBuildPatch = isBuildPatch;
        }
    }
}