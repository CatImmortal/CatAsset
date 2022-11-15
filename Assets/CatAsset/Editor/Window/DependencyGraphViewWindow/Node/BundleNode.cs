using CatAsset.Runtime;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包依赖链节点
    /// </summary>
    public class BundleNode : BaseDependencyNode<BundleRuntimeInfo>
    {
        public override BundleRuntimeInfo Owner
        {
            set
            {
                base.Owner = value;
                title = Owner.Manifest.RelativePath;
            }
        }
    }
}