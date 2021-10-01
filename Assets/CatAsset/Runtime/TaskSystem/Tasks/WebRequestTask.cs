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
        UnityWebRequestAsyncOperation op;

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

        public WebRequestTask(TaskExcutor owner, string name, int priority, Action<object> completed, object userData) : base(owner, name, priority, completed, userData)
        {
        }

        public override void Execute()
        {
            UnityWebRequest uwr = UnityWebRequest.Get(Name);
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
                Completed?.Invoke(null);
            }
            else
            {
                Completed?.Invoke(op.webRequest);
            }
        }
    }

}
