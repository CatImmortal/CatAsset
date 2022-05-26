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
        
    }
}
