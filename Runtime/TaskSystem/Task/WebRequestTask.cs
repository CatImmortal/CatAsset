using System;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{

    /// <summary>
    /// Web请求任务完成回调的原型
    /// </summary>
    public delegate void WebRequestCallback(bool success,UnityWebRequest uwr,object userdata);
    
    /// <summary>
    /// Web请求任务
    /// </summary>
    public class WebRequestTask : BaseTask<WebRequestTask>
    {
        /// <summary>
        /// Web请求的uri地址
        /// </summary>
        private string uri;
        
        /// <summary>
        /// 用户自定义数据
        /// </summary>
        private object userdata;
        
        /// <summary>
        /// Web请求任务完成回调
        /// </summary>
        private WebRequestCallback onFinished;
        
        /// <summary>
        /// Web请求的异步操作对象
        /// </summary>
        private UnityWebRequestAsyncOperation operation;

        /// <inheritdoc />
        public override float Progress
        {
            get
            {
                if (operation == null)
                {
                    return 0;
                }
                return operation.progress;
            }
        }
        
        /// <inheritdoc />
        public override void Run()
        {
            UnityWebRequest uwr = UnityWebRequest.Get(uri);
            operation = uwr.SendWebRequest();
        }

        /// <inheritdoc />
        public override void Update()
        {
            if (!operation.isDone)
            {
                State = TaskState.Running;
                return;
            }

            //请求完毕
            State = TaskState.Finished;

            if (operation.webRequest.result != UnityWebRequest.Result.Success)
            {
                onFinished?.Invoke(false, operation.webRequest,userdata);
                foreach (WebRequestTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(false, operation.webRequest,task.userdata);
                }
            }
            else
            {
                onFinished?.Invoke(true, operation.webRequest,userdata);
                foreach (WebRequestTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(true,operation.webRequest,task.userdata);
                }
            }
        }

        /// <summary>
        /// 创建Web请求任务的对象
        /// </summary>
        public static WebRequestTask Create(TaskRunner owner, string name,string uri,object userdata, WebRequestCallback callback)
        {
            WebRequestTask task = ReferencePool.Get<WebRequestTask>();
            task.CreateBase(owner,name);

            task.uri = uri;
            task.userdata = userdata;
            task.onFinished = callback;
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();
            
            uri = default;
            userdata = default;
            onFinished = default;
            operation = default;
        }
    }
}