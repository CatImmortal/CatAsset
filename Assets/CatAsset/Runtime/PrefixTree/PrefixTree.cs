using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 前缀树
    /// </summary>
    [Serializable]
    public class PrefixTree
    {

        public List<PrefixTreeNode> AllNodes = new List<PrefixTreeNode>();

        /// <summary>
        /// 节点 -> 节点ID 从0开始
        /// </summary>
        [NonSerialized]
        public Dictionary<PrefixTreeNode, int> NodeToID = new Dictionary<PrefixTreeNode, int>();

        public List<int> RootIDs = new List<int>();

        [NonSerialized]
        private Dictionary<string, PrefixTreeNode> rootDict = new Dictionary<string, PrefixTreeNode>();

        public PrefixTreeNode GetOrCreateNode(string path)
        {
            string[] contents = path.Split('/');
            if (!rootDict.TryGetValue(contents[0],out var root))
            {
                root = new PrefixTreeNode();
                root.Content = contents[0];
                rootDict.Add(root.Content,root);
            }
          
            PrefixTreeNode curNode = root;
            for (int i = 1; i < contents.Length; i++)
            {
                string content = contents[i];
                curNode = curNode.GetChild(content);
            }
            return curNode;
        }

        public void PreSerialize()
        {
            NodeToID.Clear();
            AllNodes.Clear();
            
            //收集所有节点
            foreach (var pair in rootDict)
            {
                var root = pair.Value;
                CollectNode(root);
            }
            
            //记录ID
            RootIDs.Clear();
            foreach (var pair in rootDict)
            {
                var root = pair.Value;
                RootIDs.Add(NodeToID[root]);
                BuildIDRecord(root);
            }
            
        }

        private void CollectNode(PrefixTreeNode node)
        {
            NodeToID.Add(node,AllNodes.Count);
            AllNodes.Add(node);
            
            
            foreach (var pair in node.ChildDict)
            {
                PrefixTreeNode child = pair.Value;
                CollectNode(child);
            }
        }

        private void BuildIDRecord(PrefixTreeNode node)
        {
            if (node.Parent != null)
            {
                node.ParentID = NodeToID[node.Parent];
            }
          
            node.ChildIDs = new List<int>();
            foreach (var pair in node.ChildDict)
            {
                PrefixTreeNode child = pair.Value;
                node.ChildIDs.Add(NodeToID[child]);
                
                BuildIDRecord(child);
            }
        }

        public void PostDeserialize()
        {
            //将ID重建为引用
            foreach (int rootID in RootIDs)
            {
                var root = GetNode(rootID);
                rootDict.Add(root.Content,root);
                
                BuildReference(root);
            }
        }

        private void BuildReference(PrefixTreeNode node)
        {
            node.Parent = GetNode(node.ParentID);
            
            foreach (int childID in node.ChildIDs)
            {
                var child = GetNode(childID);
                node.ChildDict.Add(child.Content,child);
                
                BuildReference(child);
            }
        }

        public PrefixTreeNode GetNode(int id)
        {
            if (id < 0 || id >= AllNodes.Count)
            {
                return null;
            }
            
            return AllNodes[id];
        }
    }
}