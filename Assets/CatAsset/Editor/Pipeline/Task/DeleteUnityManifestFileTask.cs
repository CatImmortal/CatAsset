using System;
using System.IO;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 删除UnityManifest文件的任务
    /// </summary>
    public class DeleteUnityManifestFileTask : IBuildPipelineTask
    {
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private FullOutputDirectoryParam fullOutputDirectoryParam;
        
        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                string fullOutputDirectory = fullOutputDirectoryParam.FullOutputDirectory;
                
                DirectoryInfo dirInfo = new DirectoryInfo(fullOutputDirectory);
                string directory = fullOutputDirectory.Substring(fullOutputDirectory.LastIndexOf("\\") + 1);
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