using System;
using System.Collections;
using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEngine;

public class PackageOnlyExample : MonoBehaviour
{
    private bool inited;


    private GameObject go;
    
    private void Awake()
    {
        //此脚本演示的是使用单机模式下的CatAsset
        //SteamingAssets目录下只有Base组的资源

        //需要先调用CatAssetManager.CheckPackageManifest读取SteamingAssets目录内的资源清单文件，然后才能加载资源
        CatAssetManager.CheckPackageManifest((success) => {
            if (!success)
            {
                return;
            }
            Debug.Log("按A实例化预制体");
            inited = true;
        });
    }

    private void Update()
    {
        if (inited)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                CatAssetManager.LoadAssetAsync<GameObject>("Assets/BundleRes/PrefabA/A1.prefab",(
                    assetHandler =>
                    {
                        if (assetHandler.Success)
                        {
                            Debug.Log("加载GameObject");
                            go = Instantiate(assetHandler.Asset);
                
                            //绑定assetHandler到实例化的go上 这样销毁go后也会将asset也卸载了
                            go.Bind(assetHandler);
                        }
                    }));


                CatAssetManager.LoadAssetAsync<TextAsset>("Assets/BundleRes/RawText/rawText1.txt", (assetHandler =>
                {
                    if (assetHandler.Success)
                    {
                        Debug.Log($"加载原生资源 文本文件:{assetHandler.Asset.text}");
                        assetHandler.Unload();
                    }
                }));

            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("销毁GameObject");
                Destroy(go);
                go = null;
            }
        }
    }
}
