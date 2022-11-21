using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CatAsset.Editor
{
    /// <summary>
    /// 依赖链关系图
    /// </summary>
    public class DependencyChainGraphView<TOwner, TNode> : GraphView
        where TOwner : IDependencyChainOwner<TOwner>
        where TNode : BaseDependencyNode<TOwner>, new()
    {
        /// <summary>
        /// 依赖链持有者 -> 依赖链节点
        /// </summary>
        private readonly Dictionary<TOwner, TNode> depNodes = new Dictionary<TOwner, TNode>();

        //最小深度
        private int minDepth = int.MaxValue;

        public DependencyChainGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            Insert(0, new GridBackground());  //格子背景
            this.AddManipulator(new SelectionDragger());  //节点可拖动
            this.AddManipulator(new ContentDragger());  //节点图可移动
            this.AddManipulator(new RectangleSelector());  //可框选多个节点
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(TOwner info)
        {
            CreateDependencyNode(info, true);
            CreateDependencyNode(info, false);

            TNode node = depNodes[info];
            BuildConnect(node, true);
            BuildConnect(node, false);

            CalDepth(node, 0, true);
            CalDepth(node, 0, false);

            Dictionary<int, int> depthCounter = new Dictionary<int, int>();
            CalHeight(depthCounter);

            AutoLayout(depthCounter);
        }

        /// <summary>
        /// 创建依赖节点
        /// </summary>
        private void CreateDependencyNode(TOwner info, bool isUpStream)
        {
            //创建自身
            if (!depNodes.ContainsKey(info))
            {
                var node = new TNode
                {
                    Owner = info,
                };
                depNodes.Add(info, node);
                AddElement(node);
            }

            if (isUpStream)
            {
                //递归创建上游资源的节点
                foreach (TOwner upStreamInfo in info.DependencyChain.UpStream)
                {
                    CreateDependencyNode(upStreamInfo, true);
                }
            }
            else
            {
                //递归创建下游资源的节点
                foreach (TOwner downStreamInfo in info.DependencyChain.DownStream)
                {
                    CreateDependencyNode(downStreamInfo, false);
                }
            }
        }

        /// <summary>
        /// 构建节点连接
        /// </summary>
        private void BuildConnect(TNode node, bool isUpStream)
        {

            //构建自身与上下游的连接线
            Port selfInput = (Port)node.inputContainer[0];
            Port selfOutput = (Port)node.outputContainer[0];


            if (isUpStream)
            {
                foreach (TOwner upStreamInfo in node.Owner.DependencyChain.UpStream)
                {
                    TNode upStreamNode = depNodes[upStreamInfo];
                    Port output = (Port)upStreamNode.outputContainer[0];

                    var edge = selfInput.ConnectTo(output);
                    AddElement(edge);

                    BuildConnect(upStreamNode, true);
                }
            }
            else
            {
                foreach (TOwner downStreamInfo in node.Owner.DependencyChain.DownStream)
                {
                    TNode downStreamNode = depNodes[downStreamInfo];
                    Port input = (Port)downStreamNode.inputContainer[0];

                    var edge = selfOutput.ConnectTo(input);
                    AddElement(edge);

                    BuildConnect(downStreamNode, false);
                }
            }


        }

        /// <summary>
        /// 计算深度
        /// </summary>
        private void CalDepth(TNode center, int targetDepth, bool isUpStream)
        {
            //设置深度
            if (!center.Depth.HasValue
                || (isUpStream && targetDepth < center.Depth.Value)
                || (!isUpStream && targetDepth > center.Depth.Value))
            {
                //如果此节点没设置过深度
                //或者 是上游节点 且 新深度<旧深度
                //或者 是下游节点  且 新深度>旧深度
                //就更新深度

                center.Depth = targetDepth;
                minDepth = Mathf.Min(minDepth, center.Depth.Value);
            }


            //此节点上下游的深度
            int upDepth = center.Depth.Value - 1;
            int downDepth = center.Depth.Value + 1;

            if (isUpStream)
            {
                foreach (TOwner upInfo in center.Owner.DependencyChain.UpStream)
                {
                    TNode upNode = depNodes[upInfo];
                    CalDepth(upNode, upDepth, true);
                }
            }
            else
            {
                foreach (TOwner downInfo in center.Owner.DependencyChain.DownStream)
                {
                    TNode downNode = depNodes[downInfo];
                    CalDepth(downNode, downDepth, false);
                }
            }



        }

        /// <summary>
        /// 计算高度
        /// </summary>
        private void CalHeight(Dictionary<int, int> depthCounter)
        {
            foreach (var pair in depNodes)
            {
                var node = pair.Value;

                if (!depthCounter.TryGetValue(node.Depth.Value, out var height))
                {
                    height = 1;
                    depthCounter.Add(node.Depth.Value, height);
                }
                else
                {
                    height++;
                    depthCounter[node.Depth.Value] = height;
                }

                node.Height = height;
            }
        }

        /// <summary>
        /// 自动布局
        /// </summary>
        private void AutoLayout(Dictionary<int, int> depthCounter)
        {
            int addNum = -minDepth;

            foreach (var pair in depNodes)
            {
                var node = pair.Value;

                //此深度对应的最大高度
                int maxHeight = depthCounter[node.Depth.Value];

                //将深度从-n...0...+m 映射到 0...+(n+m)
                node.Depth += addNum;
                float posX = node.Depth.Value * 750;

                //将高度从0...+n 映射到 -(n/2)...0...+(n/2)
                node.Height -= maxHeight / 2;
                float posY = node.Height.Value * 250;

                Rect pos = new Rect(new Vector2(posX, posY), Vector2.zero);
                node.SetPosition(pos);
            }
        }
    }
}
