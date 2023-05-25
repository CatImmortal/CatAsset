using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包加载任务完成回调的原型
    /// </summary>
    public delegate void BundleLoadedCallback(bool success);

    /// <summary>
    /// 资源包加载任务
    /// </summary>
    public partial class LoadBundleTask : BaseTask
    {
        /// <summary>
        /// 资源包加载状态
        /// </summary>
        protected enum LoadBundleState
        {
            None,

            /// <summary>
            /// 资源包不存在于本地
            /// </summary>
            BundleNotExist,

            /// <summary>
            /// 资源包下载中
            /// </summary>
            BundleDownloading,

            /// <summary>
            /// 资源包下载结束
            /// </summary>
            BundleDownloaded,

            /// <summary>
            /// 资源包未加载
            /// </summary>
            BundleNotLoad,

            /// <summary>
            /// 资源包加载中
            /// </summary>
            BundleLoading,

            /// <summary>
            /// 资源包加载结束
            /// </summary>
            BundleLoaded,
            
            /// <summary>
            /// 内置Shader资源包未加载
            /// </summary>
            BuiltInShaderBundleNotLoad,

            /// <summary>
            /// 内置Shader资源包加载中
            /// </summary>
            BuiltInShaderBundleLoading,

            /// <summary>
            /// 内置Shader资源包加载结束
            /// </summary>
            BuiltInShaderBundleLoaded,

        }

        protected BundleLoadedCallback OnFinishedCallback;
        protected BundleRuntimeInfo BundleRuntimeInfo;

        private readonly BundleUpdatedCallback onBundleUpdatedCallback;
        private readonly BundleLoadedCallback onBuiltInShaderBundleLoadedCallback;

        protected LoadBundleState LoadState;
        private AssetBundleCreateRequest request;

        private WebRequestedCallback onBundleBinaryLoadedCallback;
        
        private float startLoadTime;
        
        /// <inheritdoc />
        public override float Progress
        {
            get
            {
                if (request == null)
                {
                    return 0;
                }

                return request.progress;
            }
        }

        public LoadBundleTask()
        {
            onBundleUpdatedCallback = OnBundleUpdated;
            onBuiltInShaderBundleLoadedCallback = OnBuiltInShaderBundleLoadedCallback;
            onBundleBinaryLoadedCallback = OnBundleBinaryLoadedCallback;
        }
        

        /// <inheritdoc />
        public override void Run()
        {
            startLoadTime = Time.realtimeSinceStartup;
            
            if (BundleRuntimeInfo.BundleState == BundleRuntimeInfo.State.InRemote)
            {
                //不在本地 需要先下载
                LoadState = LoadBundleState.BundleNotExist;
            }
            else
            {
                //在本地了 直接加载
                LoadState = LoadBundleState.BundleNotLoad;
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            //检查是否已被全部取消
            CheckAllCanceled();
            
            if (LoadState == LoadBundleState.BundleNotExist)
            {
                //资源包不存在于本地
                CheckStateWhileBundleNotExist();
            }

            if (LoadState == LoadBundleState.BundleDownloading)
            {
                //资源包下载中
                CheckStateWhileBundleDownloading();
            }

            if (LoadState == LoadBundleState.BundleDownloaded)
            {
                //资源包下载结束
                CheckStateWhileBundleDownloaded();
            }

            if (LoadState == LoadBundleState.BundleNotLoad)
            {
                //资源包未加载
                CheckStateWhileBundleNotLoad();
            }

            if (LoadState == LoadBundleState.BundleLoading)
            {
                //资源包加载中
                CheckStateWhileBundleLoading();
            }

            if (LoadState == LoadBundleState.BundleLoaded)
            {
                //资源包加载结束
                CheckStateWhileBundleLoaded();
            }
            
            if (LoadState == LoadBundleState.BuiltInShaderBundleNotLoad)
            {
                //内置Shader资源包未加载
                CheckStateWhileBuiltInShaderBundleNotLoad();
            }

            if (LoadState == LoadBundleState.BuiltInShaderBundleLoading)
            {
                //内置Shader资源包加载中
                CheckStateWhileBuiltInShaderBundleLoading();
            }

            if (LoadState == LoadBundleState.BuiltInShaderBundleLoaded)
            {
                //内置Shader资源包加载结束
                CheckStateWhileBuiltInShaderBundleLoaded();
            }
        }

        /// <inheritdoc />
        public override void OnPriorityChanged()
        {
            if (request != null)
            {
                request.priority = (int)Group.Priority;
            }
        }

        /// <summary>
        /// 资源包更新完毕回调
        /// </summary>
        private void OnBundleUpdated(BundleUpdateResult result)
        {
            if (!result.Success)
            {
                //下载失败
                LoadState = LoadBundleState.BundleLoaded;
                return;
            }

            //下载成功 检测是否需要加载内置Shader资源包
            //Debug.Log($"下载成功：{result.UpdateInfo.Info}");
            LoadState = LoadBundleState.BundleDownloaded;
        }

        /// <summary>
        /// 内置Shader资源包加载完毕回调
        /// </summary>
        private void OnBuiltInShaderBundleLoadedCallback(bool success)
        {
            LoadState = LoadBundleState.BuiltInShaderBundleLoaded;
        }
        
        /// <summary>
        /// 资源包二进制数据加载完毕回调
        /// </summary>
        private void OnBundleBinaryLoadedCallback(bool success, UnityWebRequest uwr)
        {
            if (!success)
            {
                LoadState = LoadBundleState.BundleLoaded;
                return;
            }

            byte[] bytes = uwr.downloadHandler.data;
            EncryptUtil.EncryptXOr(bytes);
            request = AssetBundle.LoadFromMemoryAsync(bytes);
        }
        
        
        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            switch (BundleRuntimeInfo.Manifest.EncryptOption)
            {

                case BundleEncryptOptions.NotEncrypt:
                    request = AssetBundle.LoadFromFileAsync(BundleRuntimeInfo.LoadPath);
                    break;
                
                case BundleEncryptOptions.Offset:
                    request = AssetBundle.LoadFromFileAsync(BundleRuntimeInfo.LoadPath,0,EncryptUtil.EncryptBytesLength);
                    break;
                
                case BundleEncryptOptions.XOr:
                    //异或加密的资源包
                    if (BundleRuntimeInfo.BundleState == BundleRuntimeInfo.State.InReadWrite ||
                        Application.platform != RuntimePlatform.Android) 
                    {
                        //存在于读写区 或 非安卓平台 可以进行IO操作 使用Stream进行解密
                        //这里不考虑WebGL平台，因为WebGL平台不会进行加密
                        BundleRuntimeInfo.Stream = new DecryptXOrStream(BundleRuntimeInfo.LoadPath, FileMode.Open, FileAccess.Read);
                        request = AssetBundle.LoadFromStreamAsync(BundleRuntimeInfo.Stream,0,1024*1024);
                    }
                    else
                    {
                        //安卓平台下 存在于只读区 使用二进制数据解密
                        CatAssetManager.AddWebRequestTask(BundleRuntimeInfo.LoadPath, BundleRuntimeInfo.LoadPath,
                            onBundleBinaryLoadedCallback, Group.Priority);
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            request.priority = (int)Group.Priority;
        }

        /// <summary>
        /// 是否异步加载结束
        /// </summary>
        protected virtual bool IsLoadDone()
        {
            return request != null && request.isDone;
        }

        /// <summary>
        /// 异步加载结束
        /// </summary>
        protected virtual void OnLoadDone()
        {
            BundleRuntimeInfo.Bundle = request.assetBundle;
        }

        /// <summary>
        /// 调用加载完毕回调
        /// </summary>
        private void CallFinished(bool success)
        {
            if (!success)
            {
                Debug.LogError($"资源包加载失败：{BundleRuntimeInfo.Manifest}");
            }
            
            foreach (LoadBundleTask task in MergedTasks)
            {
                if (!task.IsCanceled)
                {
                    task.OnFinishedCallback?.Invoke(success);
                }
            }
            
            if (success && IsAllCanceled)
            {
                //加载资源包成功后所有任务都被取消了 这个资源包没人要了 直接走卸载流程吧
                CatAssetManager.TryUnloadBundle(BundleRuntimeInfo);
            }
        }

        /// <summary>
        /// 创建资源包加载任务的对象
        /// </summary>
        public static LoadBundleTask Create(TaskRunner owner, string name,BundleManifestInfo info,BundleLoadedCallback callback)
        {
            LoadBundleTask task = ReferencePool.Get<LoadBundleTask>();
            task.CreateBase(owner,name);

            task.OnFinishedCallback = callback;
            task.BundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(info.BundleIdentifyName);

            return task;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            OnFinishedCallback = default;
            BundleRuntimeInfo = default;
            LoadState = default;
            request = default;
            startLoadTime = default;
        }
    }
}
