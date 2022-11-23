using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器信息
    /// </summary>
    [Serializable]
    public class ProfilerInfo : IReference
    {
        /// <summary>
        /// 分析器信息类型
        /// </summary>
        public ProfilerInfoType Type;

        /// <summary>
        /// 分析器资源信息列表
        /// </summary>
        public List<ProfilerAssetInfo> AssetInfoList = new List<ProfilerAssetInfo>();

        /// <summary>
        /// 分析器资源包信息列表
        /// </summary>
        public List<ProfilerBundleInfo> BundleInfoList = new List<ProfilerBundleInfo>();

        /// <summary>
        /// 分析器任务信息列表
        /// </summary>
        public List<ProfilerTaskInfo> TaskInfoList = new List<ProfilerTaskInfo>();

        /// <summary>
        /// 分析器资源组信息列表
        /// </summary>
        public List<ProfilerGroupInfo> GroupInfoList = new List<ProfilerGroupInfo>();

        /// <summary>
        /// 分析器更新器信息列表
        /// </summary>
        public List<ProfilerUpdaterInfo> UpdaterInfoList = new List<ProfilerUpdaterInfo>();

        /// <summary>
        /// 序列化
        /// </summary>
        public static byte[] Serialize(ProfilerInfo profilerInfo)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(profilerInfo));
            return bytes;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static ProfilerInfo Deserialize(byte[] bytes)
        {
            var profilerInfo = JsonUtility.FromJson<ProfilerInfo>(Encoding.UTF8.GetString(bytes));
            profilerInfo.RebuildReference();
            return profilerInfo;
        }

        /// <summary>
        /// 重建引用
        /// </summary>
        public void RebuildReference()
        {
            if (Type != ProfilerInfoType.Bundle)
            {
                //资源包的分析器信息才需要重建引用
                return;
            }

            foreach (var pbi in BundleInfoList)
            {
                //资源包依赖链
                foreach (int upPbiIndex in pbi.UpStreamIndexes)
                {
                    var upPbi = BundleInfoList[upPbiIndex];
                    pbi.DependencyChain.UpStream.Add(upPbi);
                }
                foreach (int downPbiIndex in pbi.DownStreamIndexes)
                {
                    var downPbi = BundleInfoList[downPbiIndex];
                    pbi.DependencyChain.DownStream.Add(downPbi);
                }

                foreach (int paiIndex in pbi.InMemoryAssetIndexes)
                {
                    //资源包中在内存中的资源
                    var pai = AssetInfoList[paiIndex];
                    pbi.InMemoryAssets.Add(pai);

                    //资源依赖链
                    foreach (int upPaiIndex in pai.UpStreamIndexes)
                    {
                        var upPai = AssetInfoList[upPaiIndex];
                        pai.DependencyChain.UpStream.Add(upPai);
                    }
                    foreach (int downPaiIndex in pai.DownStreamIndexes)
                    {
                        var downPai = AssetInfoList[downPaiIndex];
                        pai.DependencyChain.DownStream.Add(downPai);
                    }
                }
            }

            BundleInfoList.Sort();
        }

        public static ProfilerInfo Create(ProfilerInfoType type)
        {
            ProfilerInfo profilerInfo = ReferencePool.Get<ProfilerInfo>();
            profilerInfo.Type = type;
            return profilerInfo;
        }

        public void Clear()
        {
            Type = default;

            foreach (var pai in AssetInfoList)
            {
                ReferencePool.Release(pai);
            }
            AssetInfoList.Clear();

            foreach (var pbi in BundleInfoList)
            {
                ReferencePool.Release(pbi);
            }
            BundleInfoList.Clear();

            foreach (var pti in TaskInfoList)
            {
                ReferencePool.Release(pti);
            }
            TaskInfoList.Clear();

            foreach (var pgi in GroupInfoList)
            {
                ReferencePool.Release(pgi);
            }
            GroupInfoList.Clear();

            foreach (var pui in UpdaterInfoList)
            {
                ReferencePool.Release(pui);
            }
            UpdaterInfoList.Clear();
        }
    }
}
