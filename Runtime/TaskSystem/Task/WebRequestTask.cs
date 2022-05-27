using System;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{

    /// <summary>
    /// Web请求任务完成回调方法的原型
    /// </summary>
    public delegate void WebRequestTaskCallback(bool success,string error,UnityWebRequest uwr,object userdata);
    
    /// <summary>
    /// Web请求任务
    /// </summary>
    public class WebRequestTask : BaseTask<WebRequestTask>,IReference
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
        private WebRequestTaskCallback onFinished;
        
        /// <summary>
        /// Web请求异步操作对象
        /// </summary>
        private UnityWebRequestAsyncOperation op;

        /// <inheritdoc />
        public override float Progress
        {
            get
            {
                if (op == null)
                {
                    return 0;
                }
                return op.progress;
            }
        }
        
        /// <inheritdoc />
        public override void Run()
        {
            UnityWebRequest uwr = UnityWebRequest.Get(uri);
            op = uwr.SendWebRequest();
        }

        /// <inheritdoc />
        public override void Update()
        {
            if (!op.isDone)
            {
                State = TaskState.Executing;
                return;
            }

            //请求完毕
            State =TaskState.Finished;

            if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
            {
                onFinished?.Invoke(false, op.webRequest.error, op.webRequest,userdata);
                foreach (WebRequestTask task in mergedTasks)
                {
                    task.onFinished?.Invoke(false, op.webRequest.error, op.webRequest,userdata);
                }
            }
            else
            {
                onFinished?.Invoke(true, null, op.webRequest,userdata);
                foreach (WebRequestTask task in mergedTasks)
                {
                    task.onFinished?.Invoke(true, null, op.webRequest,userdata);
                }
            }
        }

        /// <summary>
        /// 创建Web请求任务的对象
        /// </summary>
        public static WebRequestTask Create(TaskRunner owner, string name,string uri,object userdata, WebRequestTaskCallback onFinished)
        {
            WebRequestTask task = ReferencePool.Get<WebRequestTask>();
            task.CreateBase(owner,name);

            task.uri = uri;
            task.userdata = userdata;
            task.onFinished = onFinished;
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();
            
            uri = default;
            userdata = default;
            onFinished = default;
            op = default;
        }
    }
}