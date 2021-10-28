using CatAsset;
using CatJson;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UpdatableExample : MonoBehaviour
{
    /// <summary>
    /// 资源更新地址前缀
    /// </summary>
    public string UpdateUriPrefix;

    private bool inited;

    private bool needUpdateByChapter1;
    private bool needUpdateByChapter2;

    private void Awake()
    {
        //注意，请先点击CatAsset/WebServer/NetBox2.exe打开资源服务器

        //可更新模式
        //1.先请求最新的资源版本号
        //2.根据平台，整包版本和资源版本设置资源更新uri的前缀
        //3.检查指定资源组的资源版本
        //4.下载指定资源组的所有需要更新的资源

        //请求最新的资源清单版本号
        string versionTxtUri = UpdateUriPrefix + "/version.txt";
        Debug.Log(versionTxtUri);
        UnityWebRequest uwr = UnityWebRequest.Get(versionTxtUri);
        UnityWebRequestAsyncOperation op = uwr.SendWebRequest();
        op.completed += (obj) =>
        {
            if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
            {
                Debug.Log("读取远端最新版本号失败" + op.webRequest.error);
                return;
            }
            JsonObject jo = JsonParser.ParseJson(op.webRequest.downloadHandler.text);
            int manifestVefsion = (int)jo["ManifestVersion"].Number;

            //根据平台，整包版本和资源版本设置资源更新uri的前缀
            CatAssetManager.UpdateUriPrefix = UpdateUriPrefix + "/StandaloneWindows/" + Application.version + "_" + manifestVefsion;
            Debug.Log(CatAssetManager.UpdateUriPrefix);
            Debug.Log("读取远端最新版本号成功");
            inited = true;
        };
    }

    private void Update()
    {
        if (inited)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //检查各个组的资源
                //注意，如果不传入第二个参数，即让它为默认值null，那么会检查所有资源组的资源
                CatAssetManager.CheckVersion(OnVersionChecked, "Base");
                CatAssetManager.CheckVersion(OnVersionChecked, "Chapter1");
                CatAssetManager.CheckVersion(OnVersionChecked, "Chapter2");
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                //由于Base组的资源是首包资源，所以这里只更新Chapter1和Chapter2组的
                //注意，如果不传入第二个参数，即让它为默认值null，那么会更新所有资源组的资源

                if (needUpdateByChapter1)
                {
                    CatAssetManager.UpdateAsset(OnFileDownloaded, "Chapter1");
                }
                if (needUpdateByChapter2)
                {
                    CatAssetManager.UpdateAsset(OnFileDownloaded, "Chapter2");
                }
                
            }
        }

       
    }

    private void OnVersionChecked(int count, long length, string group)
    {
        if (group == null)
        {
            group = "所有";
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("需要更新资源数：" + count);
        sb.AppendLine("总大小：" + length);
        sb.AppendLine("资源组:" + group);

        Debug.Log(sb.ToString());
        sb.Clear();

        if (group == "Chapter1")
        {
            needUpdateByChapter1 = count > 0;
        }
        else if (group == "Chapter2")
        {
            needUpdateByChapter2 = count > 0;
        }
    }

    private void OnFileDownloaded(bool success, int updatedCount, long updatedLength, int totalCount, long totalLength, string fileName, string group)
    {
        if (!success)
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("已更新数量：" + updatedCount);
        sb.AppendLine("已更新大小：" + updatedLength);
        sb.AppendLine("总数量：" + totalCount);
        sb.AppendLine("总大小：" + totalLength);
        sb.AppendLine("资源名：" + fileName);
        if (string.IsNullOrEmpty(group))
        {
            sb.AppendLine("资源组：所有");
        }
        else
        {
            sb.AppendLine("资源组：" + group);
        }

        Debug.Log(sb.ToString());
        sb.Clear();

        if (updatedCount >= totalCount)
        {
            inited = true;

            //所有资源下载结束
            if (string.IsNullOrEmpty(group))
            {
                Debug.Log("所有资源下载完毕");
            }
            else
            {
                Debug.Log(group + "组的所有资源下载完毕");
            }

            Debug.Log($"请打开{Application.persistentDataPath}查看");

        }
    }
}
