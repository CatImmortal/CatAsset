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

                CatAssetManager.LoadAsset<GameObject>("Assets/BundleRes/PrefabA/A1.prefab", ((asset,
                    result) =>
                {
                    if (asset != null)
                    {
                        Debug.Log("加载GameObject");
                        go = Instantiate(asset);
                
                        //绑定asset到实例化的go上 这样销毁go后也会将asset也卸载了
                        CatAssetManager.BindToGameObject(go, asset);
                    }
                }));

                CatAssetManager.LoadAsset<TextAsset>("Assets/BundleRes/RawText/rawText1.txt", (
                    (asset, result) =>
                    {
                        if (asset != null)
                        {
                            Debug.Log($"加载原生资源 文本文件:{asset.text}");
                            
                            //注意这里 原生资源除非是以byte[]类型获取的 否则都是被二次包装的 不是原始资源 不能直接卸载
                            //UnloadAsset需要使用原始二进制数据来卸载 需要调用result.GetAsset()
                            CatAssetManager.UnloadAsset(result.GetAsset());
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
