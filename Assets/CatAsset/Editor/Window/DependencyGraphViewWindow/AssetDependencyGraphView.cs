using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源依赖关系图
    /// </summary>
    public class AssetDependencyGraphView : GraphView
    {
        private readonly Dictionary<AssetRuntimeInfo, AssetNode> assetNodes = new Dictionary<AssetRuntimeInfo, AssetNode>();

        //最小深度
        private int minDepth = int.MaxValue;

        public AssetDependencyGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale,ContentZoomer.DefaultMaxScale);
            Insert(0,new GridBackground());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ContentDragger());
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(AssetRuntimeInfo info)
        {


            CreateAssetNode(info);

            AssetNode node = assetNodes[info];
            BuildConnect(node,new HashSet<AssetRuntimeInfo>());

            Dictionary<int, int> depthCounter = new Dictionary<int, int>();
            node.Depth = 0;
            depthCounter[0] = 0;

            CalDepth(node,depthCounter,new HashSet<AssetRuntimeInfo>());

            AutoLayout(depthCounter);
        }

        /// <summary>
        /// 创建资源节点
        /// </summary>
        private void CreateAssetNode(AssetRuntimeInfo info)
        {
            if (assetNodes.ContainsKey(info))
            {
                //已创建过 跳出
                return;
            }

            //创建自身
            var node = new AssetNode(info);
            assetNodes.Add(info,node);
            AddElement(node);

            //递归创建上游资源的节点
            foreach (AssetRuntimeInfo upStreamInfo in info.DependencyChain.UpStream)
            {
                CreateAssetNode(upStreamInfo);
            }

            //递归创建下游资源的节点
            foreach (AssetRuntimeInfo downStreamInfo in info.DependencyChain.DownStream)
            {
                CreateAssetNode(downStreamInfo);
            }


        }

        /// <summary>
        /// 构建节点连接
        /// </summary>
        private void BuildConnect(AssetNode node,HashSet<AssetRuntimeInfo> lookedNodes)
        {
            if (lookedNodes.Contains(node.Info))
            {
                //递归过了 跳过
                return;
            }

            //加入记录
            lookedNodes.Add(node.Info);

            //构建自身与上下游的连接线
            Port selfInput = (Port)node.inputContainer[0];
            Port selfOutput = (Port)node.outputContainer[0];


            foreach (AssetRuntimeInfo upStreamInfo in node.Info.DependencyChain.UpStream)
            {
                AssetNode upStreamNode = assetNodes[upStreamInfo];
                Port output = (Port)upStreamNode.outputContainer[0];

                var edge = selfInput.ConnectTo(output);
                AddElement(edge);

                BuildConnect(upStreamNode,lookedNodes);
            }
            foreach (AssetRuntimeInfo downStreamInfo in node.Info.DependencyChain.DownStream)
            {
                AssetNode downStreamNode = assetNodes[downStreamInfo];
                Port input = (Port)downStreamNode.inputContainer[0];

                var edge = selfOutput.ConnectTo(input);
                AddElement(edge);

                BuildConnect(downStreamNode,lookedNodes);
            }
        }

        /// <summary>
        /// 计算深度
        /// </summary>
        private void CalDepth(AssetNode center,Dictionary<int,int> depthCounter,HashSet<AssetRuntimeInfo> lookedNodes)
        {
            if (lookedNodes.Contains(center.Info))
            {
                return;
            }

            lookedNodes.Add(center.Info);

            int upDepth = center.Depth - 1;
            minDepth = Mathf.Min(minDepth, upDepth);
            int downDepth = center.Depth + 1;

            foreach (AssetRuntimeInfo upInfo in center.Info.DependencyChain.UpStream)
            {
                AssetNode upNode = assetNodes[upInfo];
                if (upNode.Depth == 0 || upNode.Depth < upDepth)
                {
                    upNode.Depth = upDepth;
                }

                int height = 0;
                if (depthCounter.ContainsKey(upNode.Depth))
                {
                    height = depthCounter[upNode.Depth] + 1;
                }
                else
                {
                    depthCounter[upNode.Depth] = 0;
                }
                upNode.Height = height;

                CalDepth(upNode,depthCounter,lookedNodes);
            }

            foreach (AssetRuntimeInfo downInfo in center.Info.DependencyChain.DownStream)
            {
                AssetNode downNode = assetNodes[downInfo];
                if (downNode.Depth == 0 || downNode.Depth < downDepth)
                {
                    downNode.Depth = downDepth;
                }

                int height = 0;
                if (depthCounter.ContainsKey(downNode.Depth))
                {
                    height = depthCounter[downNode.Depth] + 1;
                }else
                {
                    depthCounter[downNode.Depth] = 0;
                }
                downNode.Height = height;

                CalDepth(downNode,depthCounter,lookedNodes);
            }
        }

        /// <summary>
        /// 自动布局
        /// </summary>
        private void AutoLayout(Dictionary<int,int> depthCounter)
        {
            int addNum = -minDepth;

            foreach (var pair in assetNodes)
            {
                var node = pair.Value;
                //将深度从-n...0...+m 映射到 0...+n+m
                float posX = (node.Depth + addNum - 1) * 500;
                Debug.Log($"{node.Info.AssetManifest.Name},posX:{posX}");

                //将高度从0...+n 映射到 -(n/2)...0...+(n/2)
                int maxHeight = depthCounter[node.Depth];
                float posY = node.Height - (maxHeight / 2);
                Debug.Log($"{node.Info.AssetManifest.Name},posY:{posY}");

                Rect pos = new Rect(new Vector2(posX, posY), Vector2.zero);
                node.SetPosition(pos);
            }
        }
    }
}
