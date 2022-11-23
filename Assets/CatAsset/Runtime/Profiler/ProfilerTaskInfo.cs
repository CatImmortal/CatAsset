using System;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器任务信息
    /// </summary>
    [Serializable]
    public class ProfilerTaskInfo : IReference,IComparable<ProfilerTaskInfo>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 类型
        /// </summary>
        public string Type;

        /// <summary>
        /// 状态
        /// </summary>
        public TaskState State;

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress;

        /// <summary>
        /// 已合并任务数
        /// </summary>
        public int MergedTaskCount;

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(ProfilerTaskInfo other)
        {
            return Name.CompareTo(other.Name);
        }

        public static ProfilerTaskInfo Create(string name,string type,TaskState state,float progress,int mergedTaskCount)
        {
            ProfilerTaskInfo info = ReferencePool.Get<ProfilerTaskInfo>();
            info.Name = name;
            info.Type = type;
            info.State = state;
            info.Progress = progress;
            info.MergedTaskCount = mergedTaskCount;
            return info;
        }

        public void Clear()
        {
            Name = default;
            Type = default;
            State = default;
            Progress = default;
            MergedTaskCount = default;
        }


    }
}
