using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CatAsset.Editor
{
    public class AssetNode: Node
    {
        public readonly AssetRuntimeInfo Info;

        private Label depthLabel;
        private Label heightLabel;

        private int? depth;
        private int? height;

        /// <summary>
        /// 布局深度
        /// </summary>
        public int? Depth
        {
            get => depth;
            set
            {
                depth = value;
                if (depthLabel != null)
                {
                    depthLabel.text = $"深度:{depth}";
                }

            }
        }

        /// <summary>
        /// 布局高度
        /// </summary>
        public int? Height  {
            get => height;
            set
            {
                height = value;
                if (heightLabel != null)
                {
                    heightLabel.text = $"高度:{height}";
                }
            }
        }

        public AssetNode(AssetRuntimeInfo info)
        {
            Info = info;
            title = info.AssetManifest.Name;

            ObjectField objectField = new ObjectField();
            objectField.value = AssetDatabase.LoadAssetAtPath<Object>(info.AssetManifest.Name);
            mainContainer.Add(objectField);

            // depthLabel = new Label();
            // heightLabel = new Label();
            // mainContainer.Add(depthLabel);
            // mainContainer.Add(heightLabel);

            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Port));
            inputPort.portName = $"上游:{info.DependencyChain.UpStream.Count}";
            inputContainer.Add(inputPort);

            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Port));
            outputPort.portName = $"下游:{info.DependencyChain.DownStream.Count}";
            outputContainer.Add(outputPort);
        }
    }
}
