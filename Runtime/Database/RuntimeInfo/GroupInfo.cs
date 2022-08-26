using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源组信息
    /// </summary>
    public class GroupInfo
    {
        /// <summary>
        /// 资源组名
        /// </summary>
        public string GroupName;

        /// <summary>
        /// 此资源组的所有远端资源包名
        /// </summary>
        public List<string> RemoteBundles = new List<string>();

        /// <summary>
        /// 此资源组的所有远端资源包数量
        /// </summary>
        public int RemoteCount;

        /// <summary>
        /// 此资源组的所有远端资源包长度
        /// </summary>
        public long RemoteLength;


        /// <summary>
        /// 此资源组的所有本地资源包名
        /// </summary>
        public List<string> LocalBundles = new List<string>();

        /// <summary>
        /// 此资源组的所有本地资源包数量
        /// </summary>
        public int LocalCount;

        /// <summary>
        /// 此资源组的所有本地资源包长度
        /// </summary>
        public long LocalLength;
    }
}