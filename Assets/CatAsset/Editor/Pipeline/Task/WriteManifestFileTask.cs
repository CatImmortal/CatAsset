using System;
using System.IO;
using CatAsset.Runtime;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 写入资源清单文件的任务
    /// </summary>
    public class WriteManifestFileTask : IBuildPipelineTask
    {
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private FullOutputDirectoryParam fullOutputDirectoryParam;
        
        [BuildPipelineParam(ParamProp = BuildPipelineParamAttribute.Property.In)]
        private CatAssetManifestParam catAssetManifestParam;
        
        /// <inheritdoc />
        public TaskResult Run()
        {
            try
            {
                string directory = fullOutputDirectoryParam.FullOutputDirectory;
                CatAssetManifest manifest = catAssetManifestParam.Manifest;
                
                //写入清单文件json
                string json = CatJson.JsonParser.ToJson(manifest);
                using (StreamWriter sw = new StreamWriter(Path.Combine(directory, Util.ManifestFileName)))
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