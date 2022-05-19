using System;
using System.Collections.Generic;
using UnityEditor;
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
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.Android,
            BuildTarget.iOS,
        };


        private void DrawBundleBuildConfigView()
        {
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("游戏版本号：" + Application.version, GUILayout.Width(200));

                EditorGUILayout.LabelField("资源清单版本号：", GUILayout.Width(100));
                bundleBuildConfg.ManifestVersion =
                    EditorGUILayout.IntField(bundleBuildConfg.ManifestVersion, GUILayout.Width(50));
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
                               bundleBuildConfg.TargetPlatforms.Contains(targetPlatform)))
                    {
                        if (toggle.enabled)
                        {
                            if (!bundleBuildConfg.TargetPlatforms.Contains(targetPlatform))
                            {
                                bundleBuildConfg.TargetPlatforms.Add(targetPlatform);
                            }
                        }
                        else
                        {
                            bundleBuildConfg.TargetPlatforms.Remove(targetPlatform);
                        }
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("选择资源包构建设置：");
            bundleBuildConfg.Options =
                (BuildAssetBundleOptions) EditorGUILayout.EnumFlagsField(bundleBuildConfg.Options);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("资源包构建输出根目录：", GUILayout.Width(150));
                bundleBuildConfg.OutputPath = GUILayout.TextField(bundleBuildConfg.OutputPath);
                if (GUILayout.Button("选择目录", GUILayout.Width(100)))
                {
                    string folder = EditorUtility.OpenFolderPanel("选择资源包构建输出根目录", bundleBuildConfg.OutputPath, "");
                    if (folder != string.Empty)
                    {
                        bundleBuildConfg.OutputPath = folder;
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
                       bundleBuildConfg.IsCopyToReadOnlyPath))
            {
                bundleBuildConfg.IsCopyToReadOnlyPath = toggle.enabled;
            }

            if (bundleBuildConfg.IsCopyToReadOnlyPath)
            {
                EditorGUILayout.LabelField("要复制的资源组（以分号分隔，为空则全部复制）：");
                bundleBuildConfg.CopyGroup = EditorGUILayout.TextField(bundleBuildConfg.CopyGroup);
            }
            
          


            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("构建资源包", GUILayout.Width(200)))
                {
                    //检查是否选中至少一个平台
                    if (bundleBuildConfg.TargetPlatforms.Count == 0)
                    {
                        EditorUtility.DisplayDialog("提示", "至少要选择一个平台", "确认");
                        return;
                    }

                    //先刷新下资源包构建信息
                    bundleBuildConfg.RefreshBundleBuildInfos();

                    //处理多个平台
                    foreach (BuildTarget targetPlatform in bundleBuildConfg.TargetPlatforms)
                    {
                        if (!buildRawBundleOnly)
                        {
                            BuildPipeline.ExecuteBundleBuildPipeline(bundleBuildConfg, targetPlatform);
                        }
                        else
                        {
                            //仅构建原生资源包
                            BuildPipeline.ExecuteRawBundleBuildPipeline(bundleBuildConfg,targetPlatform);
                        }
                       
                    }

                    bundleBuildConfg.ManifestVersion++;
                    EditorUtility.SetDirty(bundleBuildConfg);
                    AssetDatabase.SaveAssets();
                    
                    EditorUtility.DisplayDialog("提示", "资源包构建结束", "确认");
                    return;
                }

                
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(bundleBuildConfg);
                AssetDatabase.SaveAssets();
            }
        }
    }
}