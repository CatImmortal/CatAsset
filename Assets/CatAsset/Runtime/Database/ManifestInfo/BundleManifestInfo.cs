using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包清单信息
    /// </summary>
    [Serializable]
    public class BundleManifestInfo : IComparable<BundleManifestInfo>,IEquatable<BundleManifestInfo>
    {
        private string relativePath;

        /// <summary>
        /// 相对路径
        /// </summary>
        public string RelativePath{
            get
            {
                if (relativePath == null)
                {
                    if (IsAppendHash)
                    {
                        //附加了Hash值到资源包文件名中
                        string[] nameArray = BundleName.Split('.');
                        string hashBundleName =   $"{nameArray[0]}_{Hash}.{nameArray[1]}";
                        relativePath = RuntimeUtil.GetRegularPath(Path.Combine(Directory,hashBundleName));
                    }
                    else
                    {
                        relativePath = BundleIdentifyName;
                    }
                }
                return relativePath;
            }
        }

        /// <summary>
        /// 目录名
        /// </summary>
        public string Directory;

        /// <summary>
        /// 资源包名
        /// </summary>
        public string BundleName;
        

        private string bundleIdentifyName;
        /// <summary>
        /// 资源包标识名
        /// </summary>
        public string BundleIdentifyName
        {
            get
            {
                if (bundleIdentifyName == null)
                {
                    bundleIdentifyName = RuntimeUtil.GetRegularPath(Path.Combine(Directory,BundleName));
                }

                return bundleIdentifyName;
            }
        }

        /// <summary>
        /// 资源组
        /// </summary>
        public string Group;

        /// <summary>
        /// 是否为原生资源包
        /// </summary>
        public bool IsRaw;

        /// <summary>
        /// 是否为场景资源包
        /// </summary>
        public bool IsScene;

        /// <summary>
        /// 文件长度
        /// </summary>
        public ulong Length;

        /// <summary>
        /// 文件MD5（用于文件校验）
        /// </summary>
        public string MD5;

        /// <summary>
        /// 文件Hash值（用于判断是否需要更新）
        /// </summary>
        public string Hash;
        
        /// <summary>
        /// 是否附加Hash值到资源包名中
        /// </summary>
        public bool IsAppendHash;

        /// <summary>
        /// 是否依赖内置Shader资源包
        /// </summary>
        public bool IsDependencyBuiltInShaderBundle;

        /// <summary>
        /// 加密设置
        /// </summary>
        public BundleEncryptOptions EncryptOption; 
        
        /// <summary>
        /// 资源清单信息列表
        /// </summary>
        public List<AssetManifestInfo> Assets;

        public int CompareTo(BundleManifestInfo other)
        {
            return BundleIdentifyName.CompareTo(other.BundleIdentifyName);
        }

        public override string ToString()
        {
            return BundleIdentifyName;
        }

        public bool Equals(BundleManifestInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BundleIdentifyName == other.BundleIdentifyName && Length == other.Length && Hash == other.Hash;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BundleManifestInfo)obj);
        }

        public override int GetHashCode()
        {
            return (Hash != null ? Hash.GetHashCode() : 0);
        }

        /// <summary>
        /// 序列化为二进制数据
        /// </summary>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Directory);
            writer.Write(BundleName);
            writer.Write(Group);
            writer.Write(IsRaw);
            writer.Write(IsScene);
            writer.Write(Length);
            writer.Write(MD5);
            writer.Write(Hash);
            writer.Write(IsAppendHash);
            writer.Write(IsDependencyBuiltInShaderBundle);
            writer.Write((byte)EncryptOption);
            writer.Write(Assets.Count);
            foreach (AssetManifestInfo assetManifestInfo in Assets)
            {
                assetManifestInfo.Serialize(writer);
            }
        }

        /// <summary>
        /// 从二进制数据反序列化
        /// </summary>
        public static BundleManifestInfo Deserialize(BinaryReader reader,int serializeVersion)
        {
            BundleManifestInfo info = new BundleManifestInfo();
            info.Directory = reader.ReadString();
            info.BundleName = reader.ReadString();
            info.Group = reader.ReadString();
            info.IsRaw = reader.ReadBoolean();
            info.IsScene = reader.ReadBoolean();
            info.Length = reader.ReadUInt64();
            info.MD5 = reader.ReadString();
            info.Hash = reader.ReadString();
            info.IsAppendHash = reader.ReadBoolean();
            info.IsDependencyBuiltInShaderBundle = reader.ReadBoolean();
            info.EncryptOption = (BundleEncryptOptions)reader.ReadByte();
            
            int count = reader.ReadInt32();
            info.Assets = new List<AssetManifestInfo>(count);
            for (int i = 0; i < count; i++)
            {
                AssetManifestInfo assetManifestInfo = AssetManifestInfo.Deserialize(reader,serializeVersion);
                info.Assets.Add(assetManifestInfo);
            }
            return info;
        }
    }
}

