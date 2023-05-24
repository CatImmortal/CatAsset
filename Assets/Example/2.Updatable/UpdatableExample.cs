using System;
using System.Collections;
using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEngine;
using UnityEngine.Networking;

public class WebVersion
{
    public int ManifestVersion;
}

public class UpdatableExample : MonoBehaviour
{
    
    /// <summary>
    /// 资源服务器地址
    /// </summary>
    public string AssetServerIP;

    private bool isChcked;

    private void Awake()
    {
        //注意，请先点击CatAsset/WebServer/NetBox2.exe打开资源服务器

        //可更新模式
        //1.先请求最新的资源版本号
        //2.根据平台，整包版本和资源版本设置资源更新uri的前缀
        //3.检查资源版本
        //4.下载指定资源组的所有需要更新的资源

        //请求最新的资源清单版本号
        string versionTxtUri = AssetServerIP + "/version.txt";
        Debug.Log(versionTxtUri);
        UnityWebRequest uwr = UnityWebRequest.Get(versionTxtUri);
        UnityWebRequestAsyncOperation op = uwr.SendWebRequest();
        op.completed += (obj) =>
        {
            if (op.webRequest.result == UnityWebRequest.Result.ConnectionError || op.webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("读取远端最新版本号失败：" + op.webRequest.error);
                return;
            }

            WebVersion webVersion = JsonUtility.FromJson<WebVersion>(op.webRequest.downloadHandler.text);
            int manifestVersion = webVersion.ManifestVersion;

            //根据平台，整包版本和资源版本设置资源更新uri的前缀
            string uriPrefix = $"{AssetServerIP}/StandaloneWindows64/{manifestVersion}";
            CatAssetManager.UpdateUriPrefix = uriPrefix;
            Debug.Log($"读取远端最新版本号成功，资源更新地址为：{uriPrefix}");

            //检查资源版本
            CatAssetManager.CheckVersion(OnVersionChecked);
        };
    }

    private void Update()
    {
        if (isChcked)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                List<GroupInfo> groups = CatAssetManager.GetAllGroupInfo();
                foreach (GroupInfo groupInfo in groups)
                {
                    GroupUpdater updater = CatAssetManager.GetGroupUpdater(groupInfo.GroupName);
                    if (updater != null)
                    {
                        CatAssetManager.UpdateGroup(groupInfo.GroupName,OnBundleUpdated);
                       
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                CatAssetManager.InstantiateAsync("Assets/BundleRes/Chapter1/B1.prefab");
            }
        }
    }

   

    private void OnVersionChecked(VersionCheckResult result)
    {
        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError($"资源版本检查失败：{result.Error}");
            return;
        }

        Debug.Log($"资源版本检查成功：{result}");
        isChcked = true;

        //遍历所有资源组的更新器
        List<GroupInfo> groups = CatAssetManager.GetAllGroupInfo();
        foreach (GroupInfo groupInfo in groups)
        {
            GroupUpdater updater = CatAssetManager.GetGroupUpdater(groupInfo.GroupName);
            if (updater != null)
            {
                //存在资源组对应的更新器 就说明此资源组有需要更新的资源
                Debug.Log($"{groupInfo.GroupName}组的资源需要更新，请按A更新");
            }
        }
    }
    
    private void OnBundleUpdated(BundleUpdateResult result)
    {
        if (!result.Success)
        {
            Debug.LogError($"资源组{result.Updater.GroupName}的资源{result.UpdateInfo.Info}更新失败");
            return;
        }
        
        Debug.Log($"资源组{result.Updater.GroupName}的资源{result.UpdateInfo.Info}更新成功，按S实例化预制体,\n{result}");
    }
}
