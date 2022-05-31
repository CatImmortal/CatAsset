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
           
        }));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            CatAssetManager.LoadAsset<GameObject>("Assets/BundleRes/Prefab2/TestGO 1.prefab",null,((success, asset, userdata) =>
            {
           
            }));
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            CatAssetManager.LoadAsset<Texture2D>("Assets/BundleRes/Texture/Tex1.jpg",null,((success, asset, userdata) =>
            {
           
            }));
        }
    }


}
