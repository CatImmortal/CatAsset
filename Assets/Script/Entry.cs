using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CatAsset;
public class Entry : MonoBehaviour
{
    public GameObject canvans;
    private GameObject prefab;
    private GameObject go;
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
    }

}
