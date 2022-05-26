using System;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{
    /// <summary>
    /// Web请求任务完成回调原型
    /// </summary>
    public delegate void WebRequestTaskCallback(bool success,string errorMsg,UnityWebRequest uwr,object userdata);
    
    /// <summary>
    /// Web请求任务
    /// </summary>
    public class WebRequestTask : BaseTask<WebRequestTask>
    {
        private string uri;
        private object userdata;
        private WebRequestTaskCallback onFinished;
        
        private UnityWebRequestAsyncOperation op;
      
      
        
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
        
        public WebRequestTask(TaskRunner owner, string name,string uri,object userdata, WebRequestTaskCallback onFinished) : base(owner, name)
        {
            this.uri = uri;
            this.userdata = userdata;
            this.onFinished = onFinished;
        }

        public override void Run()
        {
            UnityWebRequest uwr = UnityWebRequest.Get(uri);
            op = uwr.SendWebRequest();
        }

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
    }
}