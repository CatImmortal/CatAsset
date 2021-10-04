using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CatAsset;
using UnityEngine.Networking;
using System.IO;
using CatJson;
using System.Text;
using System;

public class Entry : MonoBehaviour
{
    public GameObject canvans;

    public string UpdateUriPrefix;

    private bool inited;

    void Start()
    {
        if (CatAssetManager.IsEditorMode)
        {
            inited = true;
            return;
        }

        if (CatAssetManager.RunMode == RunMode.PackageOnly)
        {
            CatAssetManager.CheckPackageManifest((success) =>
            {
                Debug.Log("单机模式资源清单检查完毕");
                inited = true;
            });
        }
        else
        {
            Debug.Log(Application.persistentDataPath);

            //请求最新的资源清单版本号
            string versionTxtUri = UpdateUriPrefix + "/version.txt";
            Debug.Log(versionTxtUri);
            UnityWebRequest uwr = UnityWebRequest.Get(versionTxtUri);
            UnityWebRequestAsyncOperation op = uwr.SendWebRequest();
            op.completed += (obj) =>
            {
                if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
                {
                    Debug.LogError(op.webRequest.error);
                    return;
                }
                JsonObject jo = JsonParser.ParseJson(op.webRequest.downloadHandler.text);
                int manifestVefsion = (int)jo["ManifestVersion"].Number;

                //根据平台，整包版本和资源版本设置资源更新uri的前缀
                CatAssetManager.UpdateUriPrefix = UpdateUriPrefix + "/StandaloneWindows/" + Application.version + "_" + manifestVefsion;
                Debug.Log(CatAssetManager.UpdateUriPrefix);
                CatAssetManager.CheckVersion(OnVersionChecked, "Base");
                inited = true;
            };
        }

       



       
    }

    

    private void Update()
    {
        if (inited)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                CatAssetManager.CheckVersion(OnVersionChecked);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                CatAssetManager.CheckVersion(OnVersionChecked,"Chapter1");
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                CatAssetManager.CheckVersion(OnVersionChecked, "Chapter2");
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                CatAssetManager.CheckVersion(OnVersionChecked, "Chapter3");
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                CatAssetManager.UpdateAsset(OnFileDownloaded);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                CatAssetManager.UpdateAsset(OnFileDownloaded, "Chapter1");
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                CatAssetManager.UpdateAsset(OnFileDownloaded, "Chapter2");
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                CatAssetManager.UpdateAsset(OnFileDownloaded, "Chapter3");
            }


        }

      
    }

    private void OnVersionChecked(int count,long length,string group)
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

    }

    private void OnFileDownloaded(int updatedCount,long updatedLength,int totalCount,long totalLength,string fileName ,string group)
    {
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

        if (updatedCount >= 1)
        {
            CatAssetManager.PauseUpdateAsset(group);

            StartCoroutine(Delay(3, () => {
                CatAssetManager.ResumeUpdateAsset(group);
            }));
        }

        if (updatedCount >= totalCount)
        {
            //所有资源下载结束
            if (string.IsNullOrEmpty(group))
            {
                Debug.Log("所有资源下载完毕");
            }
            else
            {
                Debug.Log(group + "组的所有资源下载完毕");
            }
           
        }
    }

    private IEnumerator Delay(float time,Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }
}
