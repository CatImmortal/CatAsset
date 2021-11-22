using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset
{
    /// <summary>
    /// Web请求任务
    /// </summary>
    public class WebRequestTask : BaseTask
    {
        private UnityWebRequestAsyncOperation op;
        private string uri;
        private Action<bool,string, UnityWebRequest> onFinished;

        internal override Delegate FinishedCallback
        {
            get
            {
                return onFinished;
            }

            set
            {
                onFinished = (Action<bool, string, UnityWebRequest>)value;
            }
        }

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

        public WebRequestTask(TaskExcutor owner, string name,string uri, Action<bool, string, UnityWebRequest> onFinished) : base(owner, name)
        {
            this.uri = uri;
            this.onFinished = onFinished;
        }

        public override void Execute()
        {
            UnityWebRequest uwr = UnityWebRequest.Get(uri);
            op = uwr.SendWebRequest();
        }

        public override void Update()
        {
            if (!op.isDone)
            {
                TaskState = TaskStatus.Executing;
                return;
            }

            //请求完毕
            TaskState = TaskStatus.Finished;

            if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
            {
                onFinished?.Invoke(false, op.webRequest.error, op.webRequest);
            }
            else
            {
                onFinished?.Invoke(true, null, op.webRequest);
            }
        }
    }

}
