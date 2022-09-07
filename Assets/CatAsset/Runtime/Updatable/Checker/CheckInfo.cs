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

            if (ReadOnlyInfo != null && ReadOnlyInfo.Equals(RemoteInfo))
            {
                //此资源包最新版本存在于只读区 需要删掉读写区那份（如果存在）
                State = CheckState.InReadOnly;
                NeedRemove = ReadWriteInfo != null;
                return;
            }

            if (ReadWriteInfo != null && ReadWriteInfo.Equals(RemoteInfo))
            {
                //此资源包最新版本存在于读写区
                State = CheckState.InReadWrite;
                NeedRemove = false;
                return;
            }
            
            //此资源包存在于远端，但本地不是最新版本或本地不存在，需要删掉读写区那份（如果存在）并更新
            State = CheckState.NeedUpdate;
            NeedRemove = ReadWriteInfo != null;
        }
    }
}