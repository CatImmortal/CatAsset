﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源清单信息
    /// </summary>
    [Serializable]
    public class AssetManifestInfo : IComparable<AssetManifestInfo>,IEquatable<AssetManifestInfo>
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string Name;

        public int NameNodeID;
        
        /// <summary>
        /// 是否是图集散图
        /// </summary>
        public bool IsAtlasPackable;
        
        /// <summary>
        /// 依赖资源名列表
        /// </summary>
        public List<string> Dependencies;

        public List<int> DependencyIDList;

        public int CompareTo(AssetManifestInfo other)
        {
            return Name.CompareTo(other.Name);
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(AssetManifestInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssetManifestInfo)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
        
        /// <summary>
        /// 序列化为二进制数据
        /// </summary>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(NameNodeID);
            writer.Write(IsAtlasPackable);
            if (DependencyIDList == null)
            {
                writer.Write(0);
                return;
            }
            writer.Write(DependencyIDList.Count);
            foreach (int id in DependencyIDList)
            {
                writer.Write(id);
            }
        }
        
        /// <summary>
        /// 从二进制数据反序列化
        /// </summary>
        public static AssetManifestInfo Deserialize(BinaryReader reader,int serializeVersion)
        {
            AssetManifestInfo info = new AssetManifestInfo();
            info.NameNodeID = reader.ReadInt32();
            info.IsAtlasPackable = reader.ReadBoolean();
            int count = reader.ReadInt32();
            if (count == 0)
            {
                return info;
            }
            info.DependencyIDList = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                info.DependencyIDList.Add(id);
            }
            return info;
        }
    }

}
