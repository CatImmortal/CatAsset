using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Editor
{
    public partial class PackageWindow
    {
        /// <summary>
        /// 是否初始化过资源预览界面
        /// </summary>
        private bool isInitAssetsPreviewView;

        /// <summary>
        /// 资源预览中的各assetbundle是否已展开
        /// </summary>
        private Dictionary<string, bool> abFoldOut = new Dictionary<string, bool>();

        private Vector2 scrollPos;

        /// <summary>
        /// 要打包的AssetBundleBuild列表
        /// </summary>
        private List<AssetBundleBuild> abBuildList;

        private MethodInfo FindTextureByTypeMI;
        private object[] paramObjs = new object[1];

        /// <summary>
        /// 初始化资源预览界面
        /// </summary>
        private void InitAssetsPreviewView()
        {
            abFoldOut.Clear();
            abBuildList = Util.PkgRuleCfg.GetAssetBundleBuildList(Util.PkgCfg.IsAnalyzeRedundancy);
            foreach (AssetBundleBuild abBuild in abBuildList)
            {
                abFoldOut[abBuild.assetBundleName] = false;
            }

            FindTextureByTypeMI = typeof(EditorGUIUtility).GetMethod("FindTextureByType", BindingFlags.NonPublic | BindingFlags.Static);

        }

        /// <summary>
        /// 绘制资源预览界面
        /// </summary>
        private void DrawAssetsPreviewView()
        {
            if (!isInitAssetsPreviewView)
            {
                InitAssetsPreviewView();
                isInitAssetsPreviewView = true;
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("刷新", GUILayout.Width(100)))
                {
                    InitAssetsPreviewView();
                }

                if (GUILayout.Button("全部展开", GUILayout.Width(100)))
                {
                    foreach (AssetBundleBuild abBuild in abBuildList)
                    {
                        abFoldOut[abBuild.assetBundleName] = true;
                    }
                }

                if (GUILayout.Button("全部收起", GUILayout.Width(100)))
                {
                    foreach (AssetBundleBuild abBuild in abBuildList)
                    {
                        abFoldOut[abBuild.assetBundleName] = false;
                    }
                }

                if (GUILayout.Button("检测循环依赖",GUILayout.Width(150)))
                {
                    LoopDependencAnalyzer.AnalyzeLoopDependenc(Util.PkgRuleCfg.GetAssetBundleBuildList(Util.PkgCfg.IsAnalyzeRedundancy));
                }

               
            }

            using (EditorGUILayout.ScrollViewScope sv = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = sv.scrollPosition;
                foreach (AssetBundleBuild abBuild in abBuildList)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        abFoldOut[abBuild.assetBundleName] = EditorGUILayout.Foldout(abFoldOut[abBuild.assetBundleName], abBuild.assetBundleName);
                        string group = AssetCollector.GetAssetBundleGroup(abBuild.assetBundleName);
                        if (group != null)
                        {
                            EditorGUILayout.LabelField("资源组：" + group);
                        }
                    }

                    if (abFoldOut[abBuild.assetBundleName] == true)
                    {
                        foreach (string assetName in abBuild.assetNames)
                        {

                            DrawAsset(assetName);

                        }
                    }
                }
            }

           
        }
   
        /// <summary>
        /// 根据Asset类型绘制
        /// </summary>
        private void DrawAsset(string assetName)
        {
            Object asset = AssetDatabase.LoadAssetAtPath(assetName, typeof(Object));
            Type assetType = asset.GetType();

            GUIContent content = new GUIContent();

            if (assetType != typeof(Texture2D))
            {
                paramObjs[0] = assetType;
                content.image = (Texture2D)FindTextureByTypeMI.Invoke(null,paramObjs);
            }
            else
            {
                content.image = EditorGUIUtility.FindTexture(assetName);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (content != null)
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(30));
                    EditorGUILayout.LabelField(content, GUILayout.Width(20));
                    EditorGUILayout.LabelField(assetName, GUILayout.Width(400));
                }
                else
                {
                    EditorGUILayout.LabelField("\t   " + assetName, GUILayout.Width(455));
                }

                if (GUILayout.Button("选中",GUILayout.Width(50)))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath(assetName,typeof(Object));
                }
            }
           
        }
    }
}

