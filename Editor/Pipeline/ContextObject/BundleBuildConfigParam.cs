using UnityEditor;

namespace CatAsset.Editor
{
    /// <inheritdoc />
    public class BundleBuildConfigParam : IBundleBuildConfigParam
    {
        public BundleBuildConfigSO Config { get; }
        public BuildTarget TargetPlatform { get; }

        public BundleBuildConfigParam(BundleBuildConfigSO config,BuildTarget targetPlatform)
        {
            Config = config;
            TargetPlatform = targetPlatform;
        }
    }
}