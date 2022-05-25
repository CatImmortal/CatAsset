using System;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{
    /// <summary>
    /// Web请求任务
    /// </summary>
    public class WebRequestTask : BaseTask<WebRequestTask>
    {
        private UnityWebRequestAsyncOperation op;
        private string uri;
        private Action<bool,string, UnityWebRequest> onFinished;
        
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
        
        public WebRequestTask(TaskRunner owner, string name,string uri, Action<bool, string, UnityWebRequest> onFinished) : base(owner, name)
        {
            this.uri = uri;
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
            State =TaskState.Executing;

            if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
            {
                onFinished?.Invoke(false, op.webRequest.error, op.webRequest);
                foreach (WebRequestTask child in childTask)
                {
                    child.onFinished?.Invoke(false, op.webRequest.error, op.webRequest);
                }
            }
            else
            {
                onFinished?.Invoke(true, null, op.webRequest);
                foreach (WebRequestTask child in childTask)
                {
                    child.onFinished?.Invoke(true, null, op.webRequest);
                }
            }
        }
    }
}