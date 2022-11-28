using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{

    /// <summary>
    /// Web请求任务完成回调的原型
    /// </summary>
    public delegate void WebRequestedCallback(bool success,UnityWebRequest uwr,object userdata);

    /// <summary>
    /// Web请求任务
    /// </summary>
    public class WebRequestTask : BaseTask
    {
        private string uri;
        private object userdata;
        private WebRequestedCallback onWebRequestedCallback;

        private UnityWebRequestAsyncOperation op;
        private const int maxRetryCount = 3;
        private int retriedCount;

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

            if (RuntimeUtil.HasWebRequestError(op.webRequest))
            {
                //下载失败 重试
                if (RetryWebRequest())
                {
                    Debug.Log($"Web请求失败准备重试：{Name},错误信息：{op.webRequest.error}，当前重试次数：{retriedCount}");
                }
                else
                {
                    //重试次数达到上限 通知失败
                    Debug.LogWarning($"Web请求失败重试次数达到上限：{Name},错误信息：{op.webRequest.error}，当前重试次数：{retriedCount}");
                    State = TaskState.Finished;
                    onWebRequestedCallback?.Invoke(false, op.webRequest,userdata);
                    foreach (WebRequestTask task in MergedTasks)
                    {
                        task.onWebRequestedCallback?.Invoke(false, op.webRequest,task.userdata);
                    }
                }
            }
            else
            {
                onWebRequestedCallback?.Invoke(true, op.webRequest,userdata);
                foreach (WebRequestTask task in MergedTasks)
                {
                    task.onWebRequestedCallback?.Invoke(true,op.webRequest,task.userdata);
                }
            }
        }

        /// <summary>
        /// 尝试重新请求
        /// </summary>
        private bool RetryWebRequest()
        {
            if (retriedCount < maxRetryCount)
            {
                //重试
                retriedCount++;
                State = TaskState.Free;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 创建Web请求任务的对象
        /// </summary>
        public static WebRequestTask Create(TaskRunner owner, string name,string uri,object userdata, WebRequestedCallback callback)
        {
            WebRequestTask task = ReferencePool.Get<WebRequestTask>();
            task.CreateBase(owner,name);

            task.uri = uri;
            task.userdata = userdata;
            task.onWebRequestedCallback = callback;

            return task;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            uri = default;
            userdata = default;
            onWebRequestedCallback = default;
            op = default;
        }
    }
}
