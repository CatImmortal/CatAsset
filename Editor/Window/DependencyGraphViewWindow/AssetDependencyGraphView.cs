using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源依赖关系图
    /// </summary>
    public class AssetDependencyGraphView : GraphView
    {
        private readonly Dictionary<AssetRuntimeInfo, AssetNode> assetNodes = new Dictionary<AssetRuntimeInfo, AssetNode>();

        public AssetDependencyGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale,ContentZoomer.DefaultMaxScale);
            Insert(0,new GridBackground());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ContentDragger());
        }

        public void Init(AssetRuntimeInfo info)
        {
            CreateAssetNode(info);

            AssetNode node = assetNodes[info];
            BuildConnect(node,new HashSet<AssetRuntimeInfo>());
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
            foreach (AssetRuntimeInfo upStreamInfo in info.UpStream)
            {
                CreateAssetNode(upStreamInfo);
            }
            
            //递归创建下游资源的节点
            foreach (AssetRuntimeInfo downStreamInfo in info.DownStream)
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
            
            
            foreach (AssetRuntimeInfo upStreamInfo in node.Info.UpStream)
            {
                AssetNode upStreamNode = assetNodes[upStreamInfo];
                Port output = (Port)upStreamNode.outputContainer[0];

                var edge = selfInput.ConnectTo(output);
                AddElement(edge);
                
                BuildConnect(upStreamNode,lookedNodes);
            }
            foreach (AssetRuntimeInfo downStreamInfo in node.Info.DownStream)
            {
                AssetNode downStreamNode = assetNodes[downStreamInfo];
                Port input = (Port)downStreamNode.inputContainer[0];

                var edge = selfOutput.ConnectTo(input);
                AddElement(edge);
                
                BuildConnect(downStreamNode,lookedNodes);
            }
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(Dictionary<string, AssetRuntimeInfo> assetRuntimeInfoDict)
        {
            foreach (KeyValuePair<string,AssetRuntimeInfo> pair in assetRuntimeInfoDict)
            {
                AssetRuntimeInfo assetRuntimeInfo = pair.Value;
                
            }
        }
    }
}