using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace CatAsset
{
    

    /// <summary>
    /// 资源更新检查信息
    /// </summary>
    public class CheckInfo
    {
        public string Name;
        public CheckStatus State;
        public bool NeedRemove;
        public BundleManifestInfo ReadOnlyInfo;
        public BundleManifestInfo ReadWriteInfo;
        public BundleManifestInfo RemoteInfo;

        public CheckInfo(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 刷新资源检查信息状态
        /// </summary>
        public void RefreshState()
        {
           
            if (RemoteInfo == null)
            {
                //该bundle不存在于远端 需要删掉读写区的那份
                State = CheckStatus.Disuse;
                NeedRemove = ReadWriteInfo != null;
                return;
            }

            //添加资源组信息
            GroupInfo groupInfo = CatAssetManager.GetOrCreateGroupInfo(RemoteInfo.Group);
            groupInfo.remoteBunldes.Add(Name);
            groupInfo.remoteCount++;
            groupInfo.remoteLength += RemoteInfo.Length;

            if (ReadOnlyInfo != null && ReadOnlyInfo.Equals(RemoteInfo))
            {
                //该bundle最新版本存在于只读区 需要删掉读写区的那份
                State = CheckStatus.InReadOnly;
                NeedRemove = ReadWriteInfo != null;

                groupInfo.localBundles.Add(Name);
                groupInfo.localCount++;
                groupInfo.localLength += RemoteInfo.Length;

                return;
            }

            if (ReadWriteInfo != null && ReadWriteInfo.Equals(RemoteInfo))
            {
                //该bundle最新版本存在于读写区
                State = CheckStatus.InReadWrite;
                NeedRemove = false;

                groupInfo.localBundles.Add(Name);
                groupInfo.localCount++;
                groupInfo.localLength += RemoteInfo.Length;

                return;
            }

            //该bundle存在于远端也存在于本地，但不是最新版本，需要删掉读写区那份，并更新
            State = CheckStatus.NeedUpdate;
            NeedRemove = ReadWriteInfo != null;
        }
   

    }
}

