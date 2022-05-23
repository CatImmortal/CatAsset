namespace CatAsset.Editor
{
    /// <summary>
    /// 构建管线任务接口
    /// </summary>
    public interface IBuildPipelineTask
    {
        /// <summary>
        /// 运行构建管线任务
        /// </summary>
        TaskResult Run();
    }
}