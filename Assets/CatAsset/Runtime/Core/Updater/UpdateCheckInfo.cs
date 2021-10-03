using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace CatAsset
{
    /// <summary>
    /// 资源更新检查信息
    /// </summary>
    public class UpdateCheckInfo
    {
        public string Name;
        public UpdateCheckState State;
        public bool NeedRemove;
        public AssetBundleManifestInfo ReadOnlyInfo;
        public AssetBundleManifestInfo ReadWriteInfo;
        public AssetBundleManifestInfo RemoteInfo;

        public UpdateCheckInfo(string name)
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
                //该ab不存在于远端 需要删掉读写区的那份
                State = UpdateCheckState.Disuse;
                NeedRemove = ReadWriteInfo != null;
                return;
            }

            if (ReadOnlyInfo != null && ReadOnlyInfo.Equals(RemoteInfo))
            {
                //该ab最新版本存在于只读区 需要删掉读写区的那份
                State = UpdateCheckState.InReadOnly;
                NeedRemove = ReadWriteInfo != null;
                return;
            }

            if (ReadWriteInfo != null && ReadWriteInfo.Equals(RemoteInfo))
            {
                //该ab最新版本存在于读写区
                State = UpdateCheckState.InReadWrite;
                NeedRemove = false;
                return;
            }

            //该ab存在于远端也存在于本地，但不是最新版本，需要删掉读写区那份，并更新
            State = UpdateCheckState.NeedUpdate;
            NeedRemove = ReadWriteInfo != null;
        }
    }
}

