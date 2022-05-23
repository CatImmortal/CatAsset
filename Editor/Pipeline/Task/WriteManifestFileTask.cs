using System;
using System.IO;
using UnityEngine;

namespace CatAsset.Editor.Task
{
    /// <summary>
    /// 写入资源清单文件的任务
    /// </summary>
    public class WriteManifestFileTask : IBuildPipelineTask
    {
        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                string fullOutputPath = BuildPipelineRunner.GetPipelineParam<string>(BuildPipeline.FullOutputPath);
                CatAssetManifest manifest =
                    BuildPipelineRunner.GetPipelineParam<CatAssetManifest>(nameof(CatAssetManifest));
                
                //写入清单文件json
                string json = CatJson.JsonParser.ToJson(manifest);
                using (StreamWriter sw = new StreamWriter(Path.Combine(fullOutputPath, Util.ManifestFileName)))
                {
                    sw.Write(json);
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