using System;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包更新信息
    /// </summary>
    public class UpdateInfo : IEquatable<UpdateInfo>
    {
        /// <summary>
        /// 资源包清单信息
        /// </summary>
        public BundleManifestInfo Info;
        
        /// <summary>
        /// 状态
        /// </summary>
        public UpdateState State;

        /// <summary>
        /// 已下载字节数
        /// </summary>
        public ulong DownloadedBytesLength;

        public UpdateInfo(BundleManifestInfo info, UpdateState state)
        {
            Info = info;
            State = state;
        }

        public bool Equals(UpdateInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Info, other.Info);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UpdateInfo)obj);
        }

        public override int GetHashCode()
        {
            return (Info != null ? Info.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"UpdateInfo {Info.BundleIdentifyName}, State:{State},UpdatedLength:{DownloadedBytesLength}";
        }
    }
}