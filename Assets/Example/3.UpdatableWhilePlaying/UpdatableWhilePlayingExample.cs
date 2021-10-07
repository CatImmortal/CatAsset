using CatAsset;
using CatJson;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UpdatableWhilePlayingExample : MonoBehaviour
{
    /// <summary>
    /// 资源更新地址前缀
    /// </summary>
    public string UpdateUriPrefix;

    private bool inited;



    private void Awake()
    {
        //注意，请先点击CatAsset/WebServer/NetBox2.exe打开资源服务器
        //然后打开读写区目录删除所有文件

        //边玩边下模式
        //此模式会在加载资源时，如果资源ab不在本地但是在远端存在，会从远端下载(需要至少checkVersion过一次，但checkGroup不必是被加载的资源的group)
        
        //1.先请求最新的资源版本号
        //2.根据平台，整包版本和资源版本设置资源更新uri的前缀
        //3.加载本地不存在Chapter1的prefab


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

            CatAssetManager.CheckVersion(OnVersionChecked, "Base");
        };
    }

    private void Update()
    {
        if (inited)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //加载本地不存在的Chapter1的Cube
                CatAssetManager.LoadAsset("Assets/Res/Chapter1/Prefabs/Cube.prefab", (success, asset) =>
                {
                    if (success)
                    {
                        Instantiate(asset);
                        Debug.Log($"加载Chapter1的Cube成功，请打开{Application.persistentDataPath}目录查看资源文件");
                    }
                    else
                    {
                        Debug.Log("加载Chapter1的Cube失败");
                    }

                });
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

        inited = true;
    }
}
