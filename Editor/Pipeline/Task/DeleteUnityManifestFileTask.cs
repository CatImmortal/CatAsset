using System;
using System.IO;
using UnityEngine;

namespace CatAsset.Editor.Task
{
    /// <summary>
    /// 删除UnityManifest文件的任务
    /// </summary>
    public class DeleteUnityManifestFileTask : IBuildPipelineTask
    {
        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                string fullOutputPath = BuildPipelineRunner.GetPipelineParam<string>(BuildPipeline.FullOutputPath);
                
                DirectoryInfo dirInfo = new DirectoryInfo(fullOutputPath);
                string directory = fullOutputPath.Substring(fullOutputPath.LastIndexOf("\\") + 1);
                foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    if (file.Name == directory || file.Extension == ".manifest")
                    {
                        //删除manifest文件
                        file.Delete();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return TaskResult.Failed;
            }

            return TaskResult.Success;
        }
    }
}