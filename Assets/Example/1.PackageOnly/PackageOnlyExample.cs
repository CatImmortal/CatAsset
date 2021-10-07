using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CatAsset;

public class PackageOnlyExample : MonoBehaviour
{

    private bool inited;

    private GameObject prefab;
    private GameObject cube;

    private void Awake()
    {
        //此脚本演示的是使用单机模式下的CatAsset
        //SteamingAssets目录下只有Base组的资源

        //需要先调用CatAssetManager.CheckPackageManifest读取SteamingAssets目录内的资源清单文件，然后才能加载资源
        CatAssetManager.CheckPackageManifest((success) => {
            if (!success)
            {
                Debug.LogError("单机模式检查资源清单失败");
                return;
            }

            Debug.Log("单机模式检查资源清单成功");
            inited = true;
        });
    }

    private void Update()
    {
        if (inited)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //加载Cube的预制体
                CatAssetManager.LoadAsset("Assets/Res/Base/Prefabs/Cube.prefab", (success, asset) =>
                {
                    if (success)
                    {
                        Debug.Log("加载Cube");
                        prefab = (GameObject)asset;
                        cube = Instantiate(prefab);
                    }
                   
                });
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                //卸载Cube的预制体
                Debug.Log("卸载Cube");
                Destroy(cube);
                CatAssetManager.UnloadAsset(prefab);
            }
        }

       
    }
}
