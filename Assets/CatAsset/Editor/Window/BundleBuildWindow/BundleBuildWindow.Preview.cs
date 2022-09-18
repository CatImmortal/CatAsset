using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Editor
{
    public partial class BundleBuildWindow
    {
        /// <summary>
        /// 资源包相对路径->是否展开
        /// </summary>
        private Dictionary<string, bool> foldOutDict = new Dictionary<string, bool>();

        private Vector2 scrollPos;
        private MethodInfo findTextureByTypeMI = typeof(EditorGUIUtility).GetMethod("FindTextureByType", BindingFlags.NonPublic | BindingFlags.Static);
        private object[] paramObjs = new object[1];
        
        /// <summary>
        /// 绘制资源包预览界面
        /// </summary>
        private void DrawBundlePreviewView()
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("刷新", GUILayout.Width(100)))
                {
                    bundleBuildConfig.RefreshBundleBuildInfos();
                }

                if (GUILayout.Button("全部展开", GUILayout.Width(100)))
                {
                    foreach (BundleBuildInfo bundleBuildInfo in bundleBuildConfig.Bundles)
                    {
                        foldOutDict[bundleBuildInfo.RelativePath] = true;
                    }
                }

                if (GUILayout.Button("全部收起", GUILayout.Width(100)))
                {
                    foreach (BundleBuildInfo bundleBuildInfo in bundleBuildConfig.Bundles)
                    {
                        foldOutDict[bundleBuildInfo.RelativePath] = false;
                    }
                }
                
                if (GUILayout.Button("检测资源循环依赖",GUILayout.Width(150)))
                {
                    LoopDependencyAnalyzer.AnalyzeAsset(bundleBuildConfig.Bundles);
                }
                
                if (GUILayout.Button("检测资源包循环依赖",GUILayout.Width(150)))
                {
                    LoopDependencyAnalyzer.AnalyzeBundle(bundleBuildConfig.Bundles);
                }
                    
                bundleBuildConfig.IsRedundancyAnalyze = GUILayout.Toggle(bundleBuildConfig.IsRedundancyAnalyze, "冗余分析", GUILayout.Width(100));
            }


          
            
            using (EditorGUILayout.ScrollViewScope sv = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = sv.scrollPosition;
                
                
                foreach (BundleBuildInfo bundleBuildInfo in bundleBuildConfig.Bundles)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        //绘制展开箭头
                        foldOutDict.TryGetValue(bundleBuildInfo.RelativePath, out bool foldOut);
                        foldOutDict[bundleBuildInfo.RelativePath] = EditorGUILayout.Foldout(foldOut, bundleBuildInfo.RelativePath);


                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("|  资源组：" + bundleBuildInfo.Group,GUILayout.Width(100));
                            EditorGUILayout.LabelField("|  资源数：" + bundleBuildInfo.Assets.Count,GUILayout.Width(100));
                            EditorGUILayout.LabelField("|  总长度：" + Runtime.Util.GetByteLengthDesc(bundleBuildInfo.AssetsLength),GUILayout.Width(200));
                        }
                        
                    }
                    
                    if (foldOutDict[bundleBuildInfo.RelativePath])
                    {
                        //展开状态下 绘制资源包中的所有资源
                        foreach (AssetBuildInfo assetBuildInfo in bundleBuildInfo.Assets)
                        {
                            DrawAsset(assetBuildInfo);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 绘制资源
        /// </summary>
        private void DrawAsset(AssetBuildInfo assetBuildInfo)
        {

            Type assetType = assetBuildInfo.Type;

            GUIContent content = new GUIContent();

            if (assetType != typeof(Texture2D))
            {
                paramObjs[0] = assetType;
                content.image = (Texture2D)findTextureByTypeMI.Invoke(null,paramObjs);
            }
            else
            {
                content.image = EditorGUIUtility.FindTexture(assetBuildInfo.Name);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("", GUILayout.Width(30));
                EditorGUILayout.LabelField(content, GUILayout.Width(20));
                EditorGUILayout.LabelField(assetBuildInfo.Name, GUILayout.Width(400));
                EditorGUILayout.LabelField($"|  长度：{Runtime.Util.GetByteLengthDesc(assetBuildInfo.Length)}", GUILayout.Width(200));
                if (GUILayout.Button("选中", GUILayout.Width(50)))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath(assetBuildInfo.Name,assetBuildInfo.Type);
                }
            }
           
        }
    }
}