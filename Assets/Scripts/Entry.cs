using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CatAsset.Runtime;
using UnityEngine.SceneManagement;

public class Entry : MonoBehaviour
{

    public Scene scene1;
    public Scene scene2;
    public Scene scene3;
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
            //SceneManager.UnloadSceneAsync("Assets/BundleRes/Scene/TestScene.unity");
            int id = CatAssetManager.LoadAsset("Assets/BundleRes/PrefabA/A1.prefab",null,null);
            CatAssetManager.CancelTask(id);
            
            id = CatAssetManager.LoadAsset("Assets/BundleRes/PrefabA/A1.prefab",null,null);
            CatAssetManager.CancelTask(id);
            
            id = CatAssetManager.LoadAsset("Assets/BundleRes/PrefabA/A1.prefab",null,null);
            CatAssetManager.CancelTask(id);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {

        }

        
    }



}

public class A
{
    private int b;

    public void Test(A a)
    {
        b = 1;
        a.b = 1;
    }
}
