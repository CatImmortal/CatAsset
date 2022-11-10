using CatAsset.Runtime;
using UnityEditor.Experimental.GraphView;

namespace CatAsset.Editor
{
    public class AssetNode: Node
    {
        public AssetRuntimeInfo Info;
        
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