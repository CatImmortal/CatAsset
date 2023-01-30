using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 前缀树节点
    /// </summary>
    [Serializable]
    public class PrefixTreeNode
    {
        private static List<string> cachedList = new List<string>();
        private static StringBuilder cachedSB = new StringBuilder();
        
        public string Content;

        public int ParentID = -1;
        
        [NonSerialized]
        public PrefixTreeNode Parent;
        
        public List<int> ChildIDs;

        [NonSerialized]
        public Dictionary<string, PrefixTreeNode> ChildDict;

        public PrefixTreeNode GetChild(string content)
        {
            if (ChildDict == null)
            {
                ChildDict = new Dictionary<string, PrefixTreeNode>();
            }
            
            if (!ChildDict.TryGetValue(content,out var child))
            {
                child = new PrefixTreeNode();
                child.Content = content;
                child.Parent = this;
                ChildDict.Add(content,child);
            }

            return child;
        }
        
        public override string ToString()
        {
            PrefixTreeNode cur = this;

            int counter = 0;
            while (cur != null)
            {
                cachedList.Add(cur.Content);
                cur = cur.Parent;

                counter++;
                if (counter >= 100)
                {
                    Debug.LogError($"{Content}溢出");
                    break;
                }
            }

            for (int i = cachedList.Count - 1; i >= 0; i--)
            {
                string content = cachedList[i];
                cachedSB.Append(content);

                cachedSB.Append('/');
            }

            cachedSB.Remove(cachedSB.Length - 1,1); //删除最后多出来的一个 / 
            cachedList.Clear();
            string str = cachedSB.ToString();
            cachedSB.Clear();
            return str;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Content);
            writer.Write(ParentID);
            if (ChildIDs == null)
            {
                writer.Write(0);
                return;
            }
            writer.Write(ChildIDs.Count);
            foreach (int id in ChildIDs)
            {
                writer.Write(id);
            }
        }

        public static PrefixTreeNode Deserialize(BinaryReader reader, int serializeVersion)
        {
            PrefixTreeNode node = new PrefixTreeNode();
            try
            {
                node.Content = reader.ReadString();
                node.ParentID = reader.ReadInt32();
                int count = reader.ReadInt32();
                if (count == 0)
                {
                    return node;
                }

                node.ChildIDs = new List<int>(count);
                for (int i = 0; i < count; i++)
                {
                    int id = reader.ReadInt32();
                    node.ChildIDs.Add(id);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"前缀树节点{node.Content}反序列化失败:{e}");
                throw;
            }
           
            

            return node;
        }
    }
}