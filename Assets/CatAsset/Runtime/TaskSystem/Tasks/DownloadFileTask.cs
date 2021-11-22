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

        /// <summary>
        /// AssetBundle清单信息
        /// </summary>
        private BundleManifestInfo bundleInfo;

        /// <summary>
        /// 发起此下载任务的更新器
        /// </summary>
        private Updater updater;

        /// <summary>
        /// 下载地址
        /// </summary>
        private string downloadUri;

        /// <summary>
        /// 本地文件路径
        /// </summary>
        private string localFilePath;

        /// <summary>
        /// 本地临时文件路径
        /// </summary>
        private string localTempFilePath;

        private Action<bool,BundleManifestInfo> onFinished;

        internal override Delegate FinishedCallback
        {
            get
            {
                return onFinished;
            }

            set
            {
                onFinished = (Action<bool, BundleManifestInfo>)value;
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

        public DownloadFileTask(TaskExcutor owner, string name,BundleManifestInfo bundleInfo,Updater updater, string localFilePath, string downloadUri, Action<bool,BundleManifestInfo> onFinished) : base(owner, name)
        {
            this.bundleInfo = bundleInfo;
            this.updater = updater;
            this.localFilePath = localFilePath;
            this.downloadUri = downloadUri;
            this.onFinished = onFinished;
        }

        public override void Execute()
        {

            if (updater.state == UpdaterStatus.Paused)
            {
                //处理下载暂停 暂停只对还未开始执行的下载任务有效
                return;
            }

            localTempFilePath = localFilePath + ".downloading";

            //开始位置
            int startLength = 0;

            //先检查本地是否已存在临时下载文件
            if (File.Exists(localTempFilePath))
            {
                using (FileStream fs = File.OpenWrite(localTempFilePath))
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
            uwr.downloadHandler = new DownloadHandlerFile(localTempFilePath, startLength > 0);
            op = uwr.SendWebRequest();
        }

        public override void Update()
        {
            if (op == null)
            {
                //被暂停了
                TaskState = TaskStatus.Free;
                return;
            }

            if (!op.webRequest.isDone)
            {
                //下载中
                TaskState = TaskStatus.Executing;
                return;
            }

            //下载完毕
            TaskState = TaskStatus.Finished;

            if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
            {
                //下载失败
                Debug.LogError($"下载失败：{Name},错误信息：{op.webRequest.error}");
                onFinished?.Invoke(false , bundleInfo);
                return;
            }

            Debug.Log("下载成功：" + Name);

            //下载成功

            //TODO:文件校验


            //将临时下载文件移动到正式文件
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }

            File.Move(localTempFilePath, localFilePath);

            onFinished?.Invoke(true,bundleInfo);



        }
    }

}
