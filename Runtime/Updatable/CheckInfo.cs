namespace CatAsset.Runtime
{
    /// <summary>
    /// 版本检查信息
    /// </summary>
    public class CheckInfo
    {
        /// <summary>
        /// 资源包名
        /// </summary>
        public string Name;
        
        /// <summary>
        /// 版本检查状态
        /// </summary>
        public CheckState State;
        
        /// <summary>
        /// 是否需要删除此资源包存在于读写区的文件
        /// </summary>
        public bool NeedRemove;
        
        //此资源包的三方资源清单信息
        public BundleManifestInfo ReadOnlyInfo;
        public BundleManifestInfo ReadWriteInfo;
        public BundleManifestInfo RemoteInfo;
        
        public CheckInfo(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 刷新资源版本检查状态
        /// </summary>
        public void RefreshState()
        {
            if (RemoteInfo == null)
            {
                //此资源包不存在远端 需要删掉读写区那份（如果存在）
                State = CheckState.Disuse;
                NeedRemove = ReadWriteInfo != null;
                return;
            }
            
            GroupInfo groupInfo = CatAssetDatabase.GetOrAddGroupInfo(RemoteInfo.Group);
            
            //添加资源组的远端资源包信息
            groupInfo.RemoteBundles.Add(Name);
            groupInfo.RemoteCount++;
            groupInfo.RemoteLength += RemoteInfo.Length;

            if (ReadOnlyInfo != null && ReadOnlyInfo.Equals(RemoteInfo))
            {
                //此资源包最新版本存在于只读区 需要删掉读写区那份（如果存在）
                State = CheckState.InReadOnly;
                NeedRemove = ReadWriteInfo != null;
                
                //添加资源组的本地资源包信息
                groupInfo.LocalBundles.Add(Name);
                groupInfo.LocalCount++;
                groupInfo.LocalLength += RemoteInfo.Length;
                return;
            }

            if (ReadWriteInfo != null && ReadWriteInfo.Equals(RemoteInfo))
            {
                //此资源包最新版本存在于读写区
                State = CheckState.InReadWrite;
                NeedRemove = false;
                
                //添加资源组的本地资源包信息
                groupInfo.LocalBundles.Add(Name);
                groupInfo.LocalCount++;
                groupInfo.LocalLength += RemoteInfo.Length;
                return;
            }
            
            //此资源包存在于远端也存在于本地，但不是最新版本，需要删掉读写区那份，并更新
            State = CheckState.NeedUpdate;
            NeedRemove = ReadWriteInfo != null;
        }
    }
}