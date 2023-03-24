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
                    case LoadRawAssetState.NotLoad:
                        flag = true;
                        break;
                }

                if (flag)
                {
                    Debug.LogWarning($"{Name}被在{loadState}阶段被全部取消了");
                    State = TaskState.Finished;
                    loadState = LoadRawAssetState.None;
                    CallFinished(false);
                }
            }
        }
        
        private void CheckStateWhileNotLoad()
        {
            State = TaskState.Waiting;

            startLoadTime = Time.realtimeSinceStartup;
            webReqeustTask = WebRequestTask.Create(Owner, bundleRuntimeInfo.LoadPath, bundleRuntimeInfo.LoadPath,
                onWebRequestedCallback);
            Owner.AddTask(webReqeustTask, Group.Priority);
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
                
                if (IsAllCanceled)
                {
                    //加载成功后所有任务都被取消了 这个资源没人要了 直接走卸载流程吧
                    assetRuntimeInfo.AddRefCount();  //注意这里要先计数+1 才能正确执行后续的卸载流程
                    CatAssetManager.UnloadAsset(assetRuntimeInfo.Asset);
                }
            }
        }
    }
}