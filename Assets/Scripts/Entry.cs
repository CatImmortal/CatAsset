using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CatAsset.Runtime;

public class Entry : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CatAssetManager.CheckPackageManifest((b =>
        {
            CatAssetManager.LoadAsset("Assets/BundleRes/Prefab2/TestGO 1.prefab",null,((success, asset, userdata) =>
            {
                if (success)
                {
                    Instantiate((GameObject)asset);
                }
            }));
        }));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
