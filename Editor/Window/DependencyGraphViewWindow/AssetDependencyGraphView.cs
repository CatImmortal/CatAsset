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
            CreateAssetNode(info,true);
            CreateAssetNode(info,false);

            AssetNode node = assetNodes[info];
            BuildConnect(node,true);
            BuildConnect(node,false);

            CalDepth(node,0,true);
            CalDepth(node,0,false);

            Dictionary<int, int> depthCounter = new Dictionary<int, int>();
            CalHeight(depthCounter);
            AutoLayout(depthCounter);
        }

        /// <summary>
        /// 创建资源节点
        /// </summary>
        private void CreateAssetNode(AssetRuntimeInfo info,bool isUpStream)
        {
            //创建自身
            if (!assetNodes.ContainsKey(info))
            {
                var node = new AssetNode(info);
                assetNodes.Add(info,node);
                AddElement(node);
            }

            if (isUpStream)
            {
                //递归创建上游资源的节点
                foreach (AssetRuntimeInfo upStreamInfo in info.DependencyChain.UpStream)
                {
                    CreateAssetNode(upStreamInfo,true);
                }
            }
            else
            {
                //递归创建下游资源的节点
                foreach (AssetRuntimeInfo downStreamInfo in info.DependencyChain.DownStream)
                {
                    CreateAssetNode(downStreamInfo,false);
                }
            }
        }

        /// <summary>
        /// 构建节点连接
        /// </summary>
        private void BuildConnect(AssetNode node,bool isUpStream)
        {

            //构建自身与上下游的连接线
            Port selfInput = (Port)node.inputContainer[0];
            Port selfOutput = (Port)node.outputContainer[0];


            if (isUpStream)
            {
                foreach (AssetRuntimeInfo upStreamInfo in node.Info.DependencyChain.UpStream)
                {
                    AssetNode upStreamNode = assetNodes[upStreamInfo];
                    Port output = (Port)upStreamNode.outputContainer[0];

                    var edge = selfInput.ConnectTo(output);
                    AddElement(edge);

                    BuildConnect(upStreamNode,true);
                }
            }
            else
            {
                foreach (AssetRuntimeInfo downStreamInfo in node.Info.DependencyChain.DownStream)
                {
                    AssetNode downStreamNode = assetNodes[downStreamInfo];
                    Port input = (Port)downStreamNode.inputContainer[0];

                    var edge = selfOutput.ConnectTo(input);
                    AddElement(edge);

                    BuildConnect(downStreamNode,false);
                }
            }


        }

        /// <summary>
        /// 计算深度
        /// </summary>
        private void CalDepth(AssetNode center,int targetDepth,bool isUpStream)
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
                foreach (AssetRuntimeInfo upInfo in center.Info.DependencyChain.UpStream)
                {
                    AssetNode upNode = assetNodes[upInfo];
                    CalDepth(upNode,upDepth,true);
                }
            }
            else
            {
                foreach (AssetRuntimeInfo downInfo in center.Info.DependencyChain.DownStream)
                {
                    AssetNode downNode = assetNodes[downInfo];
                    CalDepth(downNode,downDepth,false);
                }
            }



        }

        /// <summary>
        /// 计算高度
        /// </summary>
        private void CalHeight(Dictionary<int,int> depthCounter)
        {
            foreach (var pair in assetNodes)
            {
                var node = pair.Value;

                if (!depthCounter.TryGetValue(node.Depth.Value,out var height))
                {
                    height = 1;
                    depthCounter.Add(node.Depth.Value,height);
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
        private void AutoLayout(Dictionary<int,int> depthCounter)
        {
            int addNum = -minDepth;

            foreach (var pair in assetNodes)
            {
                var node = pair.Value;

                //此深度对应的最大高度
                int maxHeight = depthCounter[node.Depth.Value];

                //将深度从-n...0...+m 映射到 0...+(n+m)
                node.Depth += addNum;
                float posX = node.Depth.Value * 750;
                Debug.Log($"{node.Info.AssetManifest.Name},depth:{node.Depth}");

                //将高度从0...+n 映射到 -(n/2)...0...+(n/2)
                node.Height -= maxHeight / 2;
                float posY = node.Height.Value * 150;
                Debug.Log($"{node.Info.AssetManifest.Name},Height:{node.Height}");

                Rect pos = new Rect(new Vector2(posX, posY), Vector2.zero);
                node.SetPosition(pos);

                Debug.Log("----------------------------------");
            }
        }
    }
}
