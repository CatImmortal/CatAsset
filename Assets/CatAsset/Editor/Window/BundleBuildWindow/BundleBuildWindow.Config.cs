using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class BundleBuildWindow
    {
        /// <summary>
        /// 是否仅构建原生资源包
        /// </summary>
        private bool buildRawBundleOnly;

        /// <summary>
        /// 可选资源包构建平台
        /// </summary>
        private List<BuildTarget> targetPlatforms = new List<BuildTarget>()
        {
            BuildTarget.StandaloneWindows64,
            BuildTarget.Android,
            BuildTarget.iOS,
            BuildTarget.WebGL,
        };


        private void DrawBundleBuildConfigView()
        {
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("游戏版本号：" + Application.version, GUILayout.Width(200));

                EditorGUILayout.LabelField("资源清单版本号：", GUILayout.Width(100));
                bundleBuildConfig.ManifestVersion =
                    EditorGUILayout.IntField(bundleBuildConfig.ManifestVersion, GUILayout.Width(50));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("选择资源包构建平台：");
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < targetPlatforms.Count; i++)
                {
                    BuildTarget targetPlatform = targetPlatforms[i];

                    using (EditorGUILayout.ToggleGroupScope toggle =
                           new EditorGUILayout.ToggleGroupScope(targetPlatform.ToString(),
                               bundleBuildConfig.TargetPlatforms.Contains(targetPlatform)))
                    {
                        if (toggle.enabled)
                        {
                            if (!bundleBuildConfig.TargetPlatforms.Contains(targetPlatform))
                            {
                                bundleBuildConfig.TargetPlatforms.Add(targetPlatform);
                            }
                        }
                        else
                        {
                            bundleBuildConfig.TargetPlatforms.Remove(targetPlatform);
                        }
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("选择资源包构建设置：");
            bundleBuildConfig.Options =
                (BundleBuildOptions) EditorGUILayout.EnumFlagsField(bundleBuildConfig.Options);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("资源包构建输出根目录：", GUILayout.Width(150));
                bundleBuildConfig.OutputPath = GUILayout.TextField(bundleBuildConfig.OutputPath);
                if (GUILayout.Button("选择目录", GUILayout.Width(100)))
                {
                    string folder = EditorUtility.OpenFolderPanel("选择资源包构建输出根目录", bundleBuildConfig.OutputPath, "");
                    if (folder != string.Empty)
                    {
                        bundleBuildConfig.OutputPath = folder;
                    }
                }
            }


            EditorGUILayout.Space();
            using (EditorGUILayout.ToggleGroupScope toggle =
                   new EditorGUILayout.ToggleGroupScope("仅构建原生资源包", buildRawBundleOnly))
            {
                buildRawBundleOnly = toggle.enabled;
            }
            

            EditorGUILayout.Space();

            using (EditorGUILayout.ToggleGroupScope toggle =
                   new EditorGUILayout.ToggleGroupScope("资源包构建目标平台只有1个时，在构建完成后将其复制到StreamingAssets目录下",
                       bundleBuildConfig.IsCopyToReadOnlyDirectory))
            {
                bundleBuildConfig.IsCopyToReadOnlyDirectory = toggle.enabled;
            }

            if (bundleBuildConfig.IsCopyToReadOnlyDirectory)
            {
                EditorGUILayout.LabelField("要复制的资源组（以分号分隔，为空则全部复制）：");
                bundleBuildConfig.CopyGroup = EditorGUILayout.TextField(bundleBuildConfig.CopyGroup);
            }
            
          

            
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("清理SBP构建缓存",GUILayout.Width(200)))
                {
                    BuildCache.PurgeCache(true);
                }

                if (GUILayout.Button("清理图集缓存",GUILayout.Width(200)))
                {
                    string atlasCachePath = "Library/AtlasCache";
                    if (!Directory.Exists(atlasCachePath))
                    {
                        Debug.Log("图集缓存目录不存在");
                        return;
                    }
                    
                    Directory.Delete(atlasCachePath, true);
                    Debug.Log("图集缓存已清理");
                }
                
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("构建资源包", GUILayout.Width(200)))
                {
                    //检查是否选中至少一个平台
                    if (bundleBuildConfig.TargetPlatforms.Count == 0)
                    {
                        EditorUtility.DisplayDialog("提示", "至少要选择一个平台", "确认");
                        return;
                    }

                    //先刷新下资源包构建信息
                    bundleBuildConfig.RefreshBundleBuildInfos();

                    //处理多个平台
                    foreach (BuildTarget targetPlatform in bundleBuildConfig.TargetPlatforms)
                    {
                        if (!buildRawBundleOnly)
                        {
                            BuildPipeline.BuildBundles(bundleBuildConfig, targetPlatform);
                        }
                        else
                        {
                            //仅构建原生资源包
                            BuildPipeline.BuildRawBundles(bundleBuildConfig,targetPlatform);
                        }
                       
                    }

                    bundleBuildConfig.ManifestVersion++;
                    EditorUtility.SetDirty(bundleBuildConfig);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    return;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(bundleBuildConfig);
                AssetDatabase.SaveAssets();
            }
        }
    }
}