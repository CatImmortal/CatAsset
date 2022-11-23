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
    public class ProfilerInfo
    {
        /// <summary>
        /// 分析器信息类型
        /// </summary>
        public ProfilerInfoType Type;

        /// <summary>
        /// 分析器资源信息列表
        /// </summary>
        public List<ProfilerAssetInfo> AssetInfoList;

        /// <summary>
        /// 分析器资源包信息列表
        /// </summary>
        public List<ProfilerBundleInfo> BundleInfoList;

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
            //还原引用
            profilerInfo.RebuildReference();

            return profilerInfo;
        }

        /// <summary>
        /// 重建引用
        /// </summary>
        public void RebuildReference()
        {
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

                foreach (int paiIndex in pbi.ReferencingAssetIndexes)
                {
                    //资源包中被引用中的资源
                    var pai = AssetInfoList[paiIndex];
                    pbi.ReferencingAssets.Add(pai);

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
        }
    }
}
