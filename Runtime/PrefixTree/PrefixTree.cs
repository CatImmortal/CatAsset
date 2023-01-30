using System;
using System.Collections.Generic;
using System.IO;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 前缀树
    /// </summary>
    [Serializable]
    public class PrefixTree
    {
        public List<PrefixTreeNode> AllNodes;
        public List<int> RootIDs;
        
        /// <summary>
        /// 节点 -> 节点ID 从0开始
        /// </summary>
        [NonSerialized]
        public Dictionary<PrefixTreeNode, int> NodeToID = new Dictionary<PrefixTreeNode, int>();

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
            AllNodes = new List<PrefixTreeNode>();
            
            //收集所有节点
            foreach (var pair in rootDict)
            {
                var root = pair.Value;
                CollectNode(root);
            }
            
            //记录ID
            RootIDs = new List<int>();
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

            if (node.ChildDict == null)
            {
                return;
            }
            
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

            if (node.ChildDict != null)
            {
                node.ChildIDs = new List<int>(node.ChildDict.Count);
                foreach (var pair in node.ChildDict)
                {
                    PrefixTreeNode child = pair.Value;
                    node.ChildIDs.Add(NodeToID[child]);
                
                    BuildIDRecord(child);
                }
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

            if (node.ChildIDs == null)
            {
                return;
            }

            node.ChildDict = new Dictionary<string, PrefixTreeNode>(node.ChildIDs.Count);
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

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(AllNodes.Count);
            foreach (PrefixTreeNode node in AllNodes)
            {
                node.Serialize(writer);
            }
            
            writer.Write(RootIDs.Count);
            foreach (int id in RootIDs)
            {
                writer.Write(id);
            }
        }

        public static PrefixTree Deserialize(BinaryReader reader, int serializeVersion)
        {
            PrefixTree prefixTree = new PrefixTree();
            int count = reader.ReadInt32();
            prefixTree.AllNodes = new List<PrefixTreeNode>(count);
            for (int i = 0; i < count; i++)
            {
                var node = PrefixTreeNode.Deserialize(reader, serializeVersion);
                prefixTree.AllNodes.Add(node);
            }

            count = reader.ReadInt32();
            prefixTree.RootIDs = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                prefixTree.RootIDs.Add(id);
            }

            return prefixTree;
        }
    }
}