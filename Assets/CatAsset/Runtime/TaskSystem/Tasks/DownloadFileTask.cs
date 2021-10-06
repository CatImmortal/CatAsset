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
        private AssetBundleManifestInfo abInfo;

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

        private Action<bool, string , AssetBundleManifestInfo> onFinished;

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

        public DownloadFileTask(TaskExcutor owner, string name,AssetBundleManifestInfo abInfo,Updater updater, string localFilePath, string downloadUri, Action<bool, string, AssetBundleManifestInfo> onFinished) : base(owner, name)
        {
            this.abInfo = abInfo;
            this.updater = updater;
            this.localFilePath = localFilePath;
            this.downloadUri = downloadUri;
            this.onFinished = onFinished;
        }

        public override void Execute()
        {

            if (updater.Paused)
            {
                //处理下载暂停
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

        public override void RefreshState()
        {
            if (op == null)
            {
                //被暂停了
                State = TaskState.Free;
                return;
            }

            if (!op.webRequest.isDone)
            {
                State = TaskState.Executing;
                return;
            }


            State = TaskState.Finished;

            if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
            {
                //下载失败
                onFinished?.Invoke(false, op.webRequest.error , abInfo);
            }
            else
            {
                //下载成功
                //将临时下载文件移动到正式文件
                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                }

                File.Move(localTempFilePath, localFilePath);

                onFinished?.Invoke(true,null, abInfo);
            }



        }
    }

}
