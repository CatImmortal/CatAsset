using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源清单
    /// </summary>
    [Serializable]
    public class CatAssetManifest
    {
        /// <summary>
        /// 资源清单Json文件名
        /// </summary>
        public const string ManifestJsonFileName = "CatAssetManifest.json";


        /// <summary>
        /// 游戏版本号
        /// </summary>
        public string GameVersion;

        /// <summary>
        /// 清单版本号
        /// </summary>
        public int ManifestVersion;

        /// <summary>
        /// 目标平台
        /// </summary>
        public string Platform;

        /// <summary>
        /// 是否附加MD5值到资源包名中
        /// </summary>
        public bool IsAppendMD5;

        /// <summary>
        /// 前缀树
        /// </summary>
        public PrefixTree PrefixTree;
        
        /// <summary>
        /// 资源包清单信息列表
        /// </summary>
        public List<BundleManifestInfo> Bundles;

        /// <summary>
        /// 资源清单信息列表
        /// </summary>
        public List<AssetManifestInfo> Assets;

        /// <summary>
        /// 序列化前的预处理方法
        /// </summary>
        private void PreSerialize()
        {
            PrefixTree = new PrefixTree();
            Assets = new List<AssetManifestInfo>();
            
            //收集所有资源
            Bundles.Sort();
            foreach (BundleManifestInfo bundleManifestInfo in Bundles)
            {
                //创建前缀树节点
                PrefixTree.GetOrCreateNode(bundleManifestInfo.Directory);
                PrefixTree.GetOrCreateNode(bundleManifestInfo.BundleName);
                PrefixTree.GetOrCreateNode(bundleManifestInfo.Group);
                
                foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
                {
                    Assets.Add(assetManifestInfo);

                    PrefixTree.GetOrCreateNode(assetManifestInfo.Name);
                    
                    foreach (string dependency in assetManifestInfo.Dependencies)
                    {
                        PrefixTree.GetOrCreateNode(dependency);
                    }
                }
            }
            
            //建立资源-ID映射
            Dictionary<AssetManifestInfo, int> assetToID = new Dictionary<AssetManifestInfo, int>();
            Assets.Sort();
            for (int i = 0; i < Assets.Count; i++)
            {
                assetToID.Add(Assets[i],i);
            }
            
            //收集前缀树节点ID
            PrefixTree.PreSerialize();
            
            //将对资源的引用变成ID
            foreach (BundleManifestInfo bundleManifestInfo in Bundles)
            {
                //记录前缀树节点ID
                var directoryNode = PrefixTree.GetOrCreateNode(bundleManifestInfo.Directory);
                var bundleNameNode = PrefixTree.GetOrCreateNode(bundleManifestInfo.BundleName);
                var groupNode = PrefixTree.GetOrCreateNode(bundleManifestInfo.Group);
                bundleManifestInfo.DirectoryNodeID = PrefixTree.NodeToID[directoryNode];
                bundleManifestInfo.BundleNameNodeID = PrefixTree.NodeToID[bundleNameNode];
                bundleManifestInfo.GroupNodeID = PrefixTree.NodeToID[groupNode];
                
                bundleManifestInfo.AssetIDs.Clear();
                foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
                {
                    bundleManifestInfo.AssetIDs.Add(assetToID[assetManifestInfo]);

                    var nameNode =  PrefixTree.GetOrCreateNode(assetManifestInfo.Name);
                    assetManifestInfo.NameNodeID = PrefixTree.NodeToID[nameNode];
                    
                    foreach (string dependency in assetManifestInfo.Dependencies)
                    {
                        var dependencyNode = PrefixTree.GetOrCreateNode(dependency);
                        var dependencyNodeID = PrefixTree.NodeToID[dependencyNode];
                        assetManifestInfo.DependencyNodeIDs.Add(dependencyNodeID);
                    }
                }
            }
        }
        
        /// <summary>
        /// 反序列化的后处理方法
        /// </summary>
        private void PostDeserialize()
        {
            PrefixTree.PostDeserialize();
            
            //从ID还原引用
            foreach (BundleManifestInfo bundleManifestInfo in Bundles)
            {
                bundleManifestInfo.Directory = PrefixTree.GetNode(bundleManifestInfo.DirectoryNodeID).ToString();
                bundleManifestInfo.BundleName = PrefixTree.GetNode(bundleManifestInfo.BundleNameNodeID).ToString();
                bundleManifestInfo.Group = PrefixTree.GetNode(bundleManifestInfo.GroupNodeID).ToString();
                
                bundleManifestInfo.IsAppendMD5 = IsAppendMD5;
                bundleManifestInfo.Assets = new List<AssetManifestInfo>(bundleManifestInfo.AssetIDs.Count);
                
                foreach (int assetID in bundleManifestInfo.AssetIDs)
                {
                    AssetManifestInfo assetManifestInfo = Assets[assetID];
                    bundleManifestInfo.Assets.Add(assetManifestInfo);

                    assetManifestInfo.Name = PrefixTree.GetNode(assetManifestInfo.NameNodeID).ToString();

                    foreach (int dependencyNodeID in assetManifestInfo.DependencyNodeIDs)
                    {
                        string dependency = PrefixTree.GetNode(dependencyNodeID).ToString();
                        assetManifestInfo.Dependencies.Add(dependency);
                    }
                }
            }
        }

        /// <summary>
        /// 序列化为二进制数据
        /// </summary>
        public byte[] SerializeToBinary()
        {
            return null;
        }

        /// <summary>
        /// 从二进制数据反序列化
        /// </summary>
        public static CatAssetManifest DeserializeFromBinary(byte[] bytes)
        {
            return null;
        }

        /// <summary>
        /// 序列化为Json文本
        /// </summary>
        public string SerializeToJson()
        {
            PreSerialize();
            string json = JsonUtility.ToJson(this,true);
            return json;
        }

        /// <summary>
        /// 从Json文本反序列化
        /// </summary>
        public static CatAssetManifest DeserializeFromJson(string json)
        {
            CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(json);
            manifest.PostDeserialize();
            return manifest;
        }
    }
}

