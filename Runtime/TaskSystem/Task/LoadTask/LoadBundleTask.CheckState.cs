using System;
using UnityEngine;

namespace CatAsset.Runtime
{
    public partial class LoadBundleTask
    {
        /// <summary>
        /// 检查是否已被全部取消
        /// </summary>
        private void CheckAllCanceled()
        {
            if (IsAllCanceled)
            {
                bool flag = false;

                switch (LoadState)
                {
                    case LoadBundleState.BundleNotExist:
                    case LoadBundleState.BundleNotLoad:
                        flag = true;
                        break;
                }

                if (flag)
                {
                    Debug.LogWarning($"{Name}被在{LoadState}阶段被全部取消了");
                    State = TaskState.Finished;
                    LoadState = LoadBundleState.None;
                    CallFinished(false);
                }
            }
        }
        
        private void CheckStateWhileBundleNotExist()
        {
            State = TaskState.Waiting;
            LoadState = LoadBundleState.BundleDownloading;

            //下载本地不存在的资源包
            //Debug.Log($"开始下载：{BundleRuntimeInfo.Manifest.RelativePath}");
            CatAssetManager.UpdateBundle(BundleRuntimeInfo.Manifest.Group, BundleRuntimeInfo.Manifest,
                onBundleUpdatedCallback);
        }

        private void CheckStateWhileBundleDownloading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWhileBundleDownloaded()
        {
            State = TaskState.Waiting;
            LoadState = LoadBundleState.BundleNotLoad;
        }


        private void CheckStateWhileBundleNotLoad()
        {
            State = TaskState.Running;
            LoadState = LoadBundleState.BundleLoading;

            LoadAsync();
        }

        private void CheckStateWhileBundleLoading()
        {
            State = TaskState.Running;

            if (IsLoadDone())
            {
                LoadState = LoadBundleState.BundleLoaded;
                LoadDone();
            }
        }

        private void CheckStateWhileBundleLoaded()
        {
            if (BundleRuntimeInfo.Bundle == null)
            {
                //加载失败
                State = TaskState.Finished;
                CallFinished(false);
            }
            else
            {
                //加载成功

                float endLoadTime = Time.realtimeSinceStartup;
                BundleRuntimeInfo.LoadTime = endLoadTime - startLoadTime;

                if (!BundleRuntimeInfo.Manifest.IsDependencyBuiltInShaderBundle)
                {
                    //不依赖内置Shader资源包 直接结束
                    State = TaskState.Finished;
                    CallFinished(true);
                }
                else
                {
                    State = TaskState.Waiting;

                    BundleRuntimeInfo builtInShaderBundleRuntimeInfo =
                        CatAssetDatabase.GetBundleRuntimeInfo(RuntimeUtil.BuiltInShaderBundleName);
                    if (builtInShaderBundleRuntimeInfo.Bundle != null)
                    {
                        //依赖内置Shader资源包 但其已加载过了 直接添加依赖链记录
                        LoadState = LoadBundleState.BuiltInShaderBundleLoaded;
                    }
                    else
                    {
                        //加载内置Shader资源包
                        LoadState = LoadBundleState.BuiltInShaderBundleNotLoad;
                    }
                }

            }
        }

        private void CheckStateWhileBuiltInShaderBundleNotLoad()
        {
            State = TaskState.Waiting;
            LoadState = LoadBundleState.BuiltInShaderBundleLoading;

            BundleRuntimeInfo builtInShaderBundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(RuntimeUtil.BuiltInShaderBundleName);
            BaseTask task;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                task = LoadWebBundleTask.Create(Owner, builtInShaderBundleRuntimeInfo.LoadPath,
                    BundleRuntimeInfo.Manifest,
                    onBuiltInShaderBundleLoadedCallback);
            }
            else
            {
                task = Create(Owner, builtInShaderBundleRuntimeInfo.LoadPath,
                    builtInShaderBundleRuntimeInfo.Manifest,
                    onBuiltInShaderBundleLoadedCallback);
            }

            Owner.AddTask(task, Group.Priority);
        }

        private void CheckStateWhileBuiltInShaderBundleLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWhileBuiltInShaderBundleLoaded()
        {
            State = TaskState.Finished;

            BundleRuntimeInfo builtInShaderBundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(RuntimeUtil.BuiltInShaderBundleName);
            if (builtInShaderBundleRuntimeInfo.Bundle != null)
            {
                //加载成功 添加依赖链记录
                builtInShaderBundleRuntimeInfo.DependencyChain.DownStream.Add(BundleRuntimeInfo);
                BundleRuntimeInfo.DependencyChain.UpStream.Add(builtInShaderBundleRuntimeInfo);
            }

            //通知主资源包加载结束
            CallFinished(true);
        }
    }
}