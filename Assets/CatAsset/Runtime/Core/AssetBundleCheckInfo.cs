using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace CatAsset
{
    /// <summary>
    /// 资源检查信息
    /// </summary>
    public class AssetBundleCheckInfo
    {
        public string Name;
        public CheckState State;
        public bool NeedRemove;
        public AssetBundleManifestInfo RemoteInfo;
        public AssetBundleManifestInfo ReadOnlyInfo;

        /// <summary>
        /// 该资源是否存在于读写区
        /// </summary>
        public bool isInReadWrite;

        public int ReadWriteLength;

        public int ReadWriteHash;

        public AssetBundleCheckInfo(string name)
        {
            Name = name;

            string readWritePath = Util.GetReadWritePath(Name);
            if (!File.Exists(readWritePath))
            {
                isInReadWrite = false;
                return;
            }

            isInReadWrite = true;
            byte[] bytes = File.ReadAllBytes(readWritePath);
            ReadWriteLength = bytes.Length;
            ReadWriteHash = Util.GetHash(bytes);
        }

        /// <summary>
        /// 刷新资源检查信息状态
        /// </summary>
        public void UpdateState()
        {
           
            if (RemoteInfo == null)
            {
                //该ab不存在于远端 需要删掉读写区的那份
                State = CheckState.Disuse;
                NeedRemove = isInReadWrite;
                return;
            }

            if (ReadOnlyInfo != null && ReadOnlyInfo.Equals(RemoteInfo))
            {
                //该ab最新版本存在于只读区 需要删掉读写区的那份
                State = CheckState.InReadOnly;
                NeedRemove = isInReadWrite;
                return;
            }

            if (isInReadWrite && RemoteInfo.Length == ReadWriteLength && RemoteInfo.Hash == ReadWriteHash)
            {
                //该ab最新版本存在于读写区
                State = CheckState.InReadWrite;
                NeedRemove = false;
                return;
            }

            //该ab存在于远端也存在于本地，但不是最新版本，需要更新
            State = CheckState.NeedUpdate;
            NeedRemove = isInReadWrite;
        }
    }
}

