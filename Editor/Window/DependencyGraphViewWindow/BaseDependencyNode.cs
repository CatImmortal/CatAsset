using System;
using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CatAsset.Editor
{
    /// <summary>
    /// 依赖链节点基类
    /// </summary>
    public abstract class BaseDependencyNode<T> : Node where T: IDependencyChainOwner<T>
    {
        /// <summary>
        /// 依赖链持有者
        /// </summary>
        private T owner;

        /// <summary>
        /// 依赖链持有者
        /// </summary>
        public virtual T Owner
        {
            get => owner;
            set
            {
                owner = value;
                inputPort.portName = $"上游:{owner.DependencyChain.UpStream.Count}";
                outputPort.portName = $"下游:{owner.DependencyChain.DownStream.Count}";
            }
        }

        private readonly Port inputPort;
        private readonly Port outputPort;
        
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
        
        public BaseDependencyNode()
        {
            // depthLabel = new Label();
            // heightLabel = new Label();
            // mainContainer.Add(depthLabel);
            // mainContainer.Add(heightLabel);

            inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Port));
            inputContainer.Add(inputPort);

            outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Port));
            outputContainer.Add(outputPort);
        }
    }
}