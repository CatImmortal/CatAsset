using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class AssetRuntimeInfoWindow
    {
        private bool isInitAssetInfoView;

        private Dictionary<string, AssetBundleRuntimeInfo> assetBundleInfoDict;
        private Dictionary<string, AssetRuntimeInfo> assetInfoDict;

        private Vector2 assetInfoScrollPos;

        /// <summary>
        /// 资源信息中的各assetbundle是否已展开
        /// </summary>
        private Dictionary<string, bool> abFoldOut = new Dictionary<string, bool>();

        /// <summary>
        /// 初始化资源信息界面
        /// </summary>
        private void InitAssetInfoView()
        {
            isInitAssetInfoView = true;
            assetBundleInfoDict = typeof(CatAssetManager).GetField("assetBundleInfoDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, AssetBundleRuntimeInfo>;
            assetInfoDict = typeof(CatAssetManager).GetField("assetInfoDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, AssetRuntimeInfo>;

        }

        /// <summary>
        /// 绘制资源信息界面
        /// </summary>
        private void DrawAssetInfoView()
        {
            if (!isInitAssetInfoView)
            {
                InitAssetInfoView();
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

            using (EditorGUILayout.ScrollViewScope sv = new EditorGUILayout.ScrollViewScope(assetInfoScrollPos))
            {
                assetInfoScrollPos = sv.scrollPosition;

                foreach (KeyValuePair<string, AssetBundleRuntimeInfo> item in assetBundleInfoDict)
                {
                    string abName = item.Key;
                    AssetBundleRuntimeInfo abInfo = item.Value;

                    //只绘制有Asset或者被其他AssetBundle依赖中的AssetBundle
                    if (abInfo.UsedAssets.Count > 0 || abInfo.RefCount > 0)
                    {

                        if (!abFoldOut.ContainsKey(abName))
                        {
                            abFoldOut.Add(abName, false);
                        }

                        if (isAllFoldOutTrue)
                        {
                            //点击过全部展开
                            abFoldOut[abName] = true;
                        }
                        else if (isAllFoldOutFalse)
                        {
                            //点击过全部收起
                            abFoldOut[abName] = false;
                        }

                        abFoldOut[abName] = EditorGUILayout.Foldout(abFoldOut[abName], abName);

                        if (abFoldOut[abName] == true)
                        {
                            foreach (AssetManifestInfo assetManifestInfo in abInfo.ManifestInfo.Assets)
                            {
                                string assetName = assetManifestInfo.AssetName;
                                AssetRuntimeInfo assetInfo = assetInfoDict[assetName];
                                if (assetInfo.RefCount == 0)
                                {
                                    continue;
                                }

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    //Asset名
                                    EditorGUILayout.LabelField(assetName, GUILayout.Width(350));

                                    //Asset类型
                                    if (assetInfo.Asset)
                                    {
                                        EditorGUILayout.LabelField(assetInfo.Asset.GetType().Name, GUILayout.Width(150));
                                    }
                                    else
                                    {
                                        EditorGUILayout.LabelField("Scene", GUILayout.Width(150));
                                    }

                                    //引用计数
                                    EditorGUILayout.LabelField("引用计数：" + assetInfo.RefCount.ToString(), GUILayout.Width(100));

                                    if (GUILayout.Button("选中", GUILayout.Width(50)))
                                    {
                                        Selection.activeObject = AssetDatabase.LoadAssetAtPath(assetName, typeof(Object));
                                    }


                                }
                            }

                            EditorGUILayout.Space();
                        }
                    }
                }
            }

           
        }
    }
}

