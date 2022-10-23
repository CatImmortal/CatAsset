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
                    
                bundleBuildConfig.IsRedundancyAnalyze = GUILayout.Toggle(bundleBuildConfig.IsRedundancyAnalyze, "冗余资源分析", GUILayout.Width(100));
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
                        
                        EditorGUILayout.LabelField("资源组：" + bundleBuildInfo.Group,GUILayout.Width(150));
                        EditorGUILayout.LabelField("资源数：" + bundleBuildInfo.Assets.Count,GUILayout.Width(100));
                        EditorGUILayout.LabelField("总长度：" + Runtime.Util.GetByteLengthDesc(bundleBuildInfo.AssetsLength),GUILayout.Width(200));
                        
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
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("\t" + assetBuildInfo.Name);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<Object>(assetBuildInfo.Name), typeof(Object),false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.LabelField($"\t长度：{Runtime.Util.GetByteLengthDesc(assetBuildInfo.Length)}");
            }
           
        }
    }
}