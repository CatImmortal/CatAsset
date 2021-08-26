using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CatAsset;
public class Entry : MonoBehaviour
{
    public GameObject canvans;
  
    // Start is called before the first frame update
    void Start()
    {
       
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            CatAssetManager.LoadAsset("Assets/Res/Analyze_1/AnalyzePrefab_1.prefab", (obj) =>
            {
                GameObject prefab = (GameObject)obj;
                GameObject go = Instantiate(prefab, canvans.transform);

            });
        }
    }

}
