using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CatAsset.Editor
{
    
    /// <summary>
    /// Shader变体收集器
    /// </summary>
    public static class ShaderVariantCollector
    {
        private const float WaitMilliseconds = 1000f;
        private static bool isCollected = false;
        private static Stopwatch sw = new Stopwatch();
        private static ShaderVariantCollection svc;
        private static void EditorUpdate()
        {
            // 注意：一定要延迟保存才会起效
            if (isCollected && sw.ElapsedMilliseconds > WaitMilliseconds)
            {
                isCollected = false;
                sw.Stop();
                EditorApplication.update -= EditorUpdate;
                
                //删除旧的svc文件
                string savePath = AssetDatabase.GetAssetPath(svc);
                AssetDatabase.DeleteAsset(savePath);
                
                // 保存结果
                SaveCurrentShaderVariantCollection(savePath);

                svc = null;
                Debug.Log($"Shader变体收集完毕");

            }
        }
        
        /// <summary>
        /// 收集变体
        /// </summary>
        public static void CollectVariant(ShaderVariantCollection svc)
        {
            if (svc == null)
            {
                return;
            }

            ShaderVariantCollector.svc = svc;

            //聚焦到Game窗口
            System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(T, false, "GameView", true);
            
            //清理旧数据
            ClearCurrentShaderVariantCollection();
            
            //创建临时场景
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

            //收集被构建为资源包的材质，用到的Shader的变体
            List<Material> materials = GetAllBundledMaterialList();
            CollectVariant(materials);
            
            EditorApplication.update += EditorUpdate;
            isCollected = true;
            sw.Reset();
            sw.Start();
        }

        /// <summary>
        /// 收集材质的Shader的变体
        /// </summary>
        private static void CollectVariant(List<Material> materials)
        {
            Camera camera = Camera.main;
            
            // 设置主相机
            float aspect = camera.aspect;
            int totalMaterials = materials.Count;
            float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
            float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
            float halfHeight = Mathf.CeilToInt(height / 2f);
            float halfWidth = Mathf.CeilToInt(width / 2f);
            camera.orthographic = true;
            camera.orthographicSize = halfHeight;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            
            // 创建测试球体
            int xMax = (int)(width - 1);
            int x = 0, y = 0;
            int progressValue = 0;
            for (int i = 0; i < materials.Count; i++)
            {
                Material material = materials[i];
                Vector3 position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
                CreateSphere(material, position, i);
                if (x == xMax)
                {
                    x = 0;
                    y++;
                }
                else
                {
                    x++;
                }

                progressValue++;
                EditorUtility.DisplayProgressBar("测试所有材质球","测试所有材质球...",(float)progressValue / materials.Count);

            }
            EditorUtility.ClearProgressBar();
        }
        
        /// <summary>
        /// 获取所有被构建为资源包的材质的列表
        /// </summary>
        public static List<Material> GetAllBundledMaterialList()
        {
            //Shader和所有用到它的材质
            List<Material> result = new List<Material>();

            BundleBuildConfigSO.Instance.RefreshBundleBuildInfos();
            int progress = 0;
            foreach (BundleBuildInfo bundleBuildInfo in BundleBuildConfigSO.Instance.Bundles)
            {
                progress++;
                EditorUtility.DisplayProgressBar("收集材质球","收集材质球...",(float)progress / BundleBuildConfigSO.Instance.Bundles.Count);
                
                if (bundleBuildInfo.IsRaw)
                {
                    //跳过原生资源包
                    continue;
                }
                foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                {
                    //跳过非材质的资源
                    if (assetBuildInfo.Type != typeof(Material))
                    {
                        continue;
                    }

                    Material material = AssetDatabase.LoadAssetAtPath<Material>(assetBuildInfo.Name);
                    if (material == null)
                    {
                        continue;
                    }

                    Shader shader = material.shader;
                    if (shader == null)
                    {
                        continue;
                    }
                    
                    result.Add(material);
                }
            }
            
            EditorUtility.ClearProgressBar();

            return result;
        }

        /// <summary>
        /// 创建测试球体
        /// </summary>
        private static void CreateSphere(Material material, Vector3 position, int index)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.GetComponent<Renderer>().material = material;
            go.transform.position = position;
            go.name = $"Sphere_{index}|{material.name}";
        }
        
        /// <summary>
        /// 清空SVC
        /// </summary>
        public static void ClearCurrentShaderVariantCollection()
        {
            EditorUtil.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
        }
        
        /// <summary>
        /// 保存SVC
        /// </summary>
        public static void SaveCurrentShaderVariantCollection(string savePath)
        {
            EditorUtil.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", savePath);
        }
        
        /// <summary>
        /// 获取SVC中的Shader数量
        /// </summary>
        public static int GetCurrentShaderVariantCollectionShaderCount()
        {
            return (int)EditorUtil.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
        }
        
        /// <summary>
        /// 获取SVC中的Shader变体数量
        /// </summary>
        public static int GetCurrentShaderVariantCollectionVariantCount()
        {
            return (int)EditorUtil.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
        }

       
    }
}