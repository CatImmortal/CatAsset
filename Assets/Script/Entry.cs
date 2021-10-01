using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CatAsset;
using UnityEngine.Networking;
using System.IO;
using CatJson;

public class Entry : MonoBehaviour
{
    public GameObject canvans;
    private GameObject prefab;
    private GameObject go;
    public string UpdateUriPrefix;


    void Start()
    {
        Debug.Log(Application.persistentDataPath);

        string versionTxtUri = UpdateUriPrefix + "/version.txt";
        Debug.Log(versionTxtUri);
        UnityWebRequest uwr = UnityWebRequest.Get(versionTxtUri);
        UnityWebRequestAsyncOperation op = uwr.SendWebRequest();
        op.completed += (obj) =>
        {
            if (op.webRequest.isNetworkError || op.webRequest.isHttpError)
            {
                Debug.LogError(op.webRequest.error);
            }
            //Debug.Log(op.webRequest.downloadHandler.text);

            JsonObject jo = JsonParser.ParseJson(op.webRequest.downloadHandler.text);

            //清单版本号
            int manifestVefsion = (int)jo["ManifestVersion"].Number;

            CatAssetUpdater.UpdateUriPrefix = UpdateUriPrefix + "/StandaloneWindows" + "/" + Application.version + "_" + manifestVefsion;
            Debug.Log(CatAssetUpdater.UpdateUriPrefix);

            CatAssetUpdater.CheckVersion((count,length) =>
            {
                Debug.Log("需要更新资源数：" + count);
                Debug.Log("总大小：" + length);

                foreach (var item in CatAssetUpdater.needUpdateList)
                {
                    Debug.Log(item);
                }

                //if (count > 0)
                //{
                //    CatAssetUpdater.UpdateAsset((updatedCount, updatedLength) => {
                //        Debug.Log("已更新数量：" + updatedCount);
                //        Debug.Log("已更新大小：" + updatedLength);
                //    });
                //}
            });
        };



       
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            CatAssetManager.LoadAsset("Assets/Res/Analyze_1/AnalyzePrefab_1.prefab", (obj) =>
            {
                prefab = (GameObject)obj;
                go = Instantiate(prefab, canvans.transform);

            });
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Destroy(go);
            CatAssetManager.UnloadAsset(prefab);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            CatAssetManager.LoadScene("Assets/Res/Scene/Scene_1.unity", null);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            CatAssetManager.UnloadScene("Assets/Res/Scene/Scene_1.unity");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            CatAssetManager.LoadAsset("Assets/Res/LoopDependency_1/LoopRef_1.prefab", (obj) =>
            {
              

            });
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            List<string> assetNames = new List<string>();
            assetNames.Add("Assets/Res/Analyze_1/AnalyzePrefab_1.prefab");
            assetNames.Add("Assets/Res/Analyze_1/AnalyzePrefab_2.prefab");
            assetNames.Add("Assets/Res/Analyze_1/AnalyzePrefab_3.prefab");
            assetNames.Add("Assets/Res/Analyze_2/AnalyzePrefab_2.prefab");
            CatAssetManager.LoadAssets(assetNames, (obj) =>
            {
                List<Object> assets = (List<Object>)obj;
                foreach (Object item in assets)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    GameObject prefab = (GameObject)item;
                    go = Instantiate(prefab, canvans.transform);
                }
            });
        }
    }

}
