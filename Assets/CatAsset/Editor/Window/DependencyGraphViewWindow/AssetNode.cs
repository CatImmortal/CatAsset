using CatAsset.Runtime;
using UnityEditor.Experimental.GraphView;

namespace CatAsset.Editor
{
    public class AssetNode: Node
    {
        public readonly AssetRuntimeInfo Info;

        /// <summary>
        /// 布局深度
        /// </summary>
        public int Depth;

        /// <summary>
        /// 布局高度
        /// </summary>
        public int Height;

        public AssetNode(AssetRuntimeInfo info)
        {
            Info = info;
            title = info.AssetManifest.Name;

            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Port));
            inputPort.portName = "上游";
            inputContainer.Add(inputPort);

            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Port));
            outputPort.portName = "下游";
            outputContainer.Add(outputPort);
        }
    }
}
