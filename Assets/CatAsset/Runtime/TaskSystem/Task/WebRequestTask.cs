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
    public class WebRequestTask : BaseTask
    {
        private string uri;
        private object userdata;
        private WebRequestCallback onFinished;
        
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
                State = TaskState.Running;
                return;
            }

            //请求完毕
            State = TaskState.Finished;

            if (op.webRequest.result != UnityWebRequest.Result.Success)
            {
                onFinished?.Invoke(false, op.webRequest,userdata);
                foreach (WebRequestTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(false, op.webRequest,task.userdata);
                }
            }
            else
            {
                onFinished?.Invoke(true, op.webRequest,userdata);
                foreach (WebRequestTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(true,op.webRequest,task.userdata);
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
            op?.webRequest.Dispose();
            op = default;
        }
    }
}