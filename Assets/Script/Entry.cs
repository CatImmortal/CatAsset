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

                //进行版本检查
                CatAssetManager.CheckVersion((count, length) =>
                {
                    Debug.Log("需要更新资源数：" + count);
                    Debug.Log("总大小：" + length);

                    if (count > 0)
                    {
                        //更新资源
                        CatAssetManager.UpdateAssets((updatedCount, updatedLength) =>
                        {
                            Debug.Log("已更新数量：" + updatedCount);
                            Debug.Log("已更新大小：" + updatedLength);

                            if (updatedCount >= count)
                            {
                                //所有资源下载结束
                                inited = true;
                            }
                        });
                    }
                    else
                    {
                        //没有资源需要更新
                        inited = true;
                    }
                });
            };
        }

       



       
    }

    private void Update()
    {
        if (inited)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                CatAssetManager.LoadAsset("Assets/Res/Analyze_1/AnalyzePrefab_1.prefab", (sucess,obj) =>
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
                CatAssetManager.LoadAsset("Assets/Res/LoopDependency_1/LoopRef_1.prefab", (sucess, obj) =>
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
                CatAssetManager.LoadAssets(assetNames, (assets) =>
                {
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

}
