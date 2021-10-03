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
        private Action<bool,string, UnityWebRequest,object> onFinished;

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

        public WebRequestTask(TaskExcutor owner, string name, int priority,object userData,string uri, Action<bool, string, UnityWebRequest, object> onFinished) : base(owner, name, priority,userData)
        {
            this.uri = uri;
            this.onFinished = onFinished;
        }

        public override void Execute()
        {
            UnityWebRequest uwr = UnityWebRequest.Get(uri);
            op = uwr.SendWebRequest();
            op.priority = Priority;
        }

        public override void UpdateState()
        {
            if (!op.isDone)
            {
                State = TaskState.Executing;
                return;
            }

            //请求完毕
            State = TaskState.Finished;

            if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
            {
                onFinished?.Invoke(false, op.webRequest.error, op.webRequest, UserData);
            }
            else
            {
                onFinished?.Invoke(true, null, op.webRequest, UserData);
            }
        }
    }

}
