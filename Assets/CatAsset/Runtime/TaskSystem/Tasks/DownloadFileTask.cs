using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
namespace CatAsset
{
    /// <summary>
    /// 下载文件任务
    /// </summary>
    public class DownloadFileTask : BaseTask
    {
        private UnityWebRequestAsyncOperation op;
        private string downloadFilePath;
        private string downloadTempFilePath;

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

        public DownloadFileTask(TaskExcutor owner, string name, int priority, Action<object> completed, object userData) : base(owner, name, priority, completed, userData)
        {
        }

        public override void Execute()
        {

            downloadFilePath = Util.GetReadWritePath(Name);
            downloadTempFilePath = downloadFilePath + ".dowloading";
            string downloadUri = (string)UserData;

            //开始位置
            int startLength = 0;

            //先检查本地是否已存在下载文件
            if (File.Exists(downloadTempFilePath))
            {
                using (FileStream fs = File.OpenWrite(downloadTempFilePath))
                {
                    //检查已下载的字节数
                    fs.Seek(0, SeekOrigin.End);
                    startLength = (int)fs.Length;
                }
               
            }

            UnityWebRequest uwr = new UnityWebRequest(downloadUri);
            if (startLength > 0)
            {
                //处理断点续传
                uwr.SetRequestHeader("Range", $"bytes={{{startLength}}}-");
            }
            uwr.downloadHandler = new DownloadHandlerFile(downloadTempFilePath, startLength > 0);
            op = uwr.SendWebRequest();
            op.priority = Priority;
        }

        public override void UpdateState()
        {
            if (!op.webRequest.isDone)
            {
                State = TaskState.Executing;
                return;
            }


            //下载完毕
            State = TaskState.Finished;

            if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
            {
                Debug.LogError($"{Name}下载失败：{op.webRequest.error}");
                Completed?.Invoke(null);
            }
            else
            {
                //将临时下载文件移动到正式文件
                if (File.Exists(downloadFilePath))
                {
                    File.Delete(downloadFilePath);
                }
                File.Move(downloadTempFilePath, downloadFilePath);
                Completed?.Invoke(op.webRequest);
            }



        }
    }

}
