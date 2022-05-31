using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CatAsset.Runtime;
using Codice.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Editor
{
    public partial class RuntimeInfoWindow
    {
        private bool isInitRuntimeInfoView;

        private Dictionary<string, BundleRuntimeInfo> bundleRuntimeInfoDict;
        private Dictionary<string, AssetRuntimeInfo> assetRuntimeInfoDict;

        private Vector2 runtimeInfo;
        
        private MethodInfo findTextureByTypeMI = typeof(EditorGUIUtility).GetMethod("FindTextureByType", BindingFlags.NonPublic | BindingFlags.Static);
        private object[] paramObjs = new object[1];
        
        /// <summary>
        /// 资源包相对路径->是否展开
        /// </summary>
        private Dictionary<string, bool> foldOut = new Dictionary<string, bool>();

        /// <summary>
        /// 初始化运行时信息界面
        /// </summary>
        private void InitRuntimeInfoView()
        {
            isInitRuntimeInfoView = true;
            bundleRuntimeInfoDict = typeof(CatAssetManager).GetField(nameof(bundleRuntimeInfoDict), BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, BundleRuntimeInfo>;
            assetRuntimeInfoDict = typeof(CatAssetManager).GetField(nameof(assetRuntimeInfoDict), BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, AssetRuntimeInfo>;

        }

        /// <summary>
        /// 绘制运行时信息界面
        /// </summary>
        private void DrawRuntimeInfoView()
        {
            if (!isInitRuntimeInfoView)
            {
                InitRuntimeInfoView();
            }
            
            bool isAllFoldOutTrue = false;
            bool isAllFoldOutFalse = false;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("全部展开", GUILayout.Width(100)))
                {
                    isAllFoldOutTrue = true;
                }

                if (GUILayout.Button("全部收起", GUILayout.Width(100)))
                {
                    isAllFoldOutFalse = true;
                }
            }

            using (EditorGUILayout.ScrollViewScope sv = new EditorGUILayout.ScrollViewScope(runtimeInfo))
            {
                runtimeInfo = sv.scrollPosition;

                foreach (KeyValuePair<string, BundleRuntimeInfo> item in bundleRuntimeInfoDict)
                {
                    string bundleRelativePath = item.Key;
                    BundleRuntimeInfo bundleRuntimeInfo = item.Value;

                    //只绘制有资源在使用中的资源包
                    if (bundleRuntimeInfo.UsedAssets.Count > 0)
                    {
                        if (!foldOut.ContainsKey(bundleRelativePath))
                        {
                            foldOut.Add(bundleRelativePath, false);
                        }

                        if (isAllFoldOutTrue)
                        {
                            //点击过全部展开
                            foldOut[bundleRelativePath] = true;
                        }
                        else if (isAllFoldOutFalse)
                        {
                            //点击过全部收起
                            foldOut[bundleRelativePath] = false;
                        }

                        foldOut[bundleRelativePath] = EditorGUILayout.Foldout(foldOut[bundleRelativePath], bundleRelativePath);

                        if (foldOut[bundleRelativePath] == true)
                        {
                            
                            foreach (AssetManifestInfo assetManifestInfo in bundleRuntimeInfo.Manifest.Assets)
                            {
                                string assetName = assetManifestInfo.Name;
                                AssetRuntimeInfo assetRuntimeInfo = assetRuntimeInfoDict[assetName];
                                if (assetRuntimeInfo.RefCount == 0)
                                {
                                    continue;
                                }
                                DrawAssetRuntimeInfo(assetRuntimeInfo);
                            }

                            EditorGUILayout.Space();
                        }
                    }
                }
            }

           
        }

        /// <summary>
        /// 绘制资源运行时信息
        /// </summary>
        private void DrawAssetRuntimeInfo(AssetRuntimeInfo assetRuntimeInfo)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string assetName = assetRuntimeInfo.AssetManifest.Name;
                
                //资源图标
                GUIContent content = new GUIContent();
                Type assetType = assetRuntimeInfo.Asset.GetType();
                if (assetType != typeof(Texture2D))
                {
                    paramObjs[0] = assetType;
                    content.image = (Texture2D) findTextureByTypeMI.Invoke(null, paramObjs);
                }
                else
                {
                    content.image = EditorGUIUtility.FindTexture(assetName);
                }

                EditorGUILayout.LabelField(content, GUILayout.Width(20));

                //资源名
                EditorGUILayout.LabelField(assetName, GUILayout.Width(350));

                //资源类型
                if (assetRuntimeInfo.Asset != null)
                {
                    EditorGUILayout.LabelField(assetRuntimeInfo.Asset.GetType().Name, GUILayout.Width(150));
                }
                else
                {
                    EditorGUILayout.LabelField("Scene", GUILayout.Width(150));
                }

                //引用计数
                EditorGUILayout.LabelField("引用计数：" + assetRuntimeInfo.RefCount.ToString(), GUILayout.Width(100));

                EditorGUILayout.LabelField("RefAsset数量：" + assetRuntimeInfo.RefAssetList.Count.ToString(), GUILayout.Width(100));
                if (GUILayout.Button("查看RefAsset", GUILayout.Width(100)))
                {
                    RefAssetListWindow.OpenWindow(this,assetRuntimeInfo);
                }
                
                if (GUILayout.Button("选中", GUILayout.Width(50)))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath(assetName, assetType);
                }
                

            }
        }

        /// <summary>
        ///  通过依赖加载引用了指定资源的资源列表窗口
        /// </summary>
        private class RefAssetListWindow : EditorWindow
        {
            private AssetRuntimeInfo assetRuntimeInfo;
            private RuntimeInfoWindow parent;
            public static void OpenWindow(RuntimeInfoWindow parent, AssetRuntimeInfo assetRuntimeInfo)
            {
                RefAssetListWindow window = CreateWindow<RefAssetListWindow>(nameof(RefAssetListWindow));
                window.ShowPopup();
                window.parent = parent;
                window.assetRuntimeInfo = assetRuntimeInfo;
                

            }

            private void DrawRefAssetList()
            {
                foreach (string assetName in assetRuntimeInfo.RefAssetList)
                {
                    AssetRuntimeInfo refAsset = CatAssetManager.GetAssetRuntimeInfo(assetName);
                    parent.DrawAssetRuntimeInfo(refAsset);
                }
            }

            private void OnGUI()
            {
                if (!Application.isPlaying)
                {
                    Close();
                    return;
                }
                
                DrawRefAssetList();
            }
        }
    }
}

