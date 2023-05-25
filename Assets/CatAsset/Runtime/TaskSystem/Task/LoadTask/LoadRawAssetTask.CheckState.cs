using System;
using UnityEngine;

namespace CatAsset.Runtime
{
    public partial class LoadRawAssetTask
    {
        /// <summary>
        /// 检查是否已被全部取消
        /// </summary>
        private void CheckAllCanceled()
        {
            if (IsAllCanceled)
            {
                bool flag = false;

                switch (loadState)
                {
                    case LoadRawAssetState.NotExist:
                    case LoadRawAssetState.NotLoad:
                        flag = true;
                        break;
                }

                if (flag)
                {
                    Debug.LogWarning($"{GetType().Name}:{Name}在{loadState}阶段被全部取消了");
                    State = TaskState.Finished;
                    loadState = LoadRawAssetState.None;
                    CallFinished(false);
                }
            }
        }
        
        private void CheckStateWhileNotExist()
        {
            State = TaskState.Waiting;
            loadState = LoadRawAssetState.Downloading;
                
            //下载本地不存在的原生资源
            //Debug.Log($"开始下载：{bundleRuntimeInfo.Manifest.RelativePath}");
            CatAssetManager.UpdateBundle(bundleRuntimeInfo.Manifest.Group, bundleRuntimeInfo.Manifest,
                onRawAssetUpdatedCallback);
        }

        private void CheckStateWhileDownloading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWhileDownloaded()
        {
            State = TaskState.Waiting;
            loadState = LoadRawAssetState.NotLoad;
        }
        
        private void CheckStateWhileNotLoad()
        {
            State = TaskState.Waiting;
            
            webRequestTask = WebRequestTask.Create(Owner, bundleRuntimeInfo.LoadPath, bundleRuntimeInfo.LoadPath,
                onWebRequestedCallback);
            Owner.AddTask(webRequestTask, Group.Priority);
        }

        private void CheckStateWhileLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWhileLoaded()
        {
            State = TaskState.Finished;

            if (assetRuntimeInfo == null)
            {
                //资源加载失败
                CallFinished(false);
            }
            else
            {
                if (startLoadTime != 0)
                {
                    //加载成功 计算加载耗时
                    float endLoadTime = Time.realtimeSinceStartup;
                    assetRuntimeInfo.LoadTime = endLoadTime - startLoadTime;
                    //Debug.Log($"{assetRuntimeInfo}加载成功，耗时:{assetRuntimeInfo.LoadTime:0.000}秒");
                }

                //资源加载成功 或 是已加载好的
                CallFinished(true);
            }
        }
    }
}