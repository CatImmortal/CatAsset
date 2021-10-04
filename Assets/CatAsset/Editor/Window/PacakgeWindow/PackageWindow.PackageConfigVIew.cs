using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class PackageWindow
    {
        /// <summary>
        /// 是否初始化过打包配置界面
        /// </summary>
        private bool isInitPackageConfigView;

        /// <summary>
        /// 可选打包平台
        /// </summary>
        private List<BuildTarget> targetPlatforms = new List<BuildTarget>()
        {
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.Android,
            BuildTarget.iOS,
        };

        /// <summary>
        /// 各平台选择状态
        /// </summary>
        private Dictionary<BuildTarget, bool> selectedPlatforms = new Dictionary<BuildTarget, bool>()
        {
            {BuildTarget.StandaloneWindows,false },
            {BuildTarget.StandaloneWindows64,false },
            {BuildTarget.Android,false},
            {BuildTarget.iOS,false},
        };

        /// <summary>
        /// 可选打包设置
        /// </summary>
        private string[] options = Enum.GetNames(typeof(BuildAssetBundleOptions));

        /// <summary>
        /// 打包设置选择状态
        /// </summary>
        private Dictionary<string, bool> selectedOptions = new Dictionary<string, bool>();



        /// <summary>
        /// 初始化打包配置界面
        /// </summary>
        private void InitPackgeConfigView()
        {
            foreach (BuildTarget item in Util.PkgCfg.TargetPlatforms)
            {
                selectedPlatforms[item] = true;
            }

            for (int i = 1; i < options.Length; i++)
            {
                BuildAssetBundleOptions option = (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), options[i]);

                if ((Util.PkgCfg.Options & option) > 0)
                {
                    selectedOptions[options[i]] = true;
                }
                else
                {
                    selectedOptions[options[i]] = false;
                }
            }
        }

        /// <summary>
        /// 保存打包配置
        /// </summary>
        private void SavePackageConfig()
        {

            Util.PkgCfg.TargetPlatforms.Clear();
            foreach (KeyValuePair<BuildTarget, bool> item in selectedPlatforms)
            {
                if (item.Value == true)
                {
                    Util.PkgCfg.TargetPlatforms.Add(item.Key);
                }
            }

            Util.PkgCfg.Options = BuildAssetBundleOptions.None;
            foreach (KeyValuePair<string, bool> item in selectedOptions)
            {
                if (item.Value == true)
                {
                    BuildAssetBundleOptions option = (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), item.Key);
                    Util.PkgCfg.Options |= option;
                }
            }

            EditorUtility.SetDirty(Util.PkgCfg);
        }

        /// <summary>
        /// 绘制打包配置界面
        /// </summary>
        private void DrawPackageConfigView()
        {
            if (!isInitPackageConfigView)
            {
                InitPackgeConfigView();
                isInitPackageConfigView = true;
            }

            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("游戏版本号：" + Application.version,GUILayout.Width(200));

                EditorGUILayout.LabelField("资源清单版本号：", GUILayout.Width(100));
                Util.PkgCfg.ManifestVersion = EditorGUILayout.IntField(Util.PkgCfg.ManifestVersion, GUILayout.Width(50));

            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("选择打包平台：");

            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < targetPlatforms.Count; i++)
                {
                    using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope(targetPlatforms[i].ToString(), selectedPlatforms[targetPlatforms[i]]))
                    {
                        selectedPlatforms[targetPlatforms[i]] = toggle.enabled;
                    }

                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("选择打包设置：");
            for (int i = 1; i < options.Length; i += 3)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int j = 0; j < 3 && i + j < options.Length; j++)
                    {
                        using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope(options[i + j], selectedOptions[options[i + j]]))
                        {
                            selectedOptions[options[i + j]] = toggle.enabled;
                        }
                    }

                }
            }

            EditorGUILayout.Space();



            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("打包输出根目录：", GUILayout.Width(100));
                Util.PkgCfg.OutputPath = GUILayout.TextField(Util.PkgCfg.OutputPath);
                if (GUILayout.Button("选择目录", GUILayout.Width(100)))
                {
                    string folder = EditorUtility.OpenFolderPanel("选择打包输出根目录", Util.PkgCfg.OutputPath, "");
                    if (folder != string.Empty)
                    {
                        Util.PkgCfg.OutputPath = folder;
                    }
                }
            }

            EditorGUILayout.Space();

            using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope("冗余分析", Util.PkgCfg.IsAnalyzeRedundancy))
            {
                Util.PkgCfg.IsAnalyzeRedundancy = toggle.enabled;
            }


            EditorGUILayout.Space();

            using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope("打包平台只选中了1个时，打包后复制资源到StreamingAssets下", Util.PkgCfg.IsCopyToStreamingAssets))
            {
                Util.PkgCfg.IsCopyToStreamingAssets = toggle.enabled;
            }
            if (Util.PkgCfg.IsCopyToStreamingAssets)
            {
                EditorGUILayout.LabelField("要复制的资源组（以分号分隔，为空则全部复制）：");
                Util.PkgCfg.CopyGroup = EditorGUILayout.TextField(Util.PkgCfg.CopyGroup);
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {

                if (GUILayout.Button("打包AssetBundle", GUILayout.Width(200)))
                {

                    //检查是否选中至少一个平台
                    if (Util.PkgCfg.TargetPlatforms.Count == 0)
                    {
                        EditorUtility.DisplayDialog("提示", "至少要选择一个打包平台", "确认");
                        return;
                    }


                    //处理多个平台的打包
                    foreach (BuildTarget item in Util.PkgCfg.TargetPlatforms)
                    {
                        if (Util.PkgCfg.TargetPlatforms.Count == 1)
                        {
                            Packager.ExecutePackagePipeline(Util.PkgCfg.OutputPath, Util.PkgCfg.Options, item, Util.PkgCfg.ManifestVersion, Util.PkgCfg.IsAnalyzeRedundancy, Util.PkgCfg.IsCopyToStreamingAssets, Util.PkgCfg.CopyGroup);
                        }
                        else
                        {
                            Packager.ExecutePackagePipeline(Util.PkgCfg.OutputPath, Util.PkgCfg.Options, item, Util.PkgCfg.ManifestVersion, Util.PkgCfg.IsAnalyzeRedundancy, false,null);
                        }
                    }


                    EditorUtility.DisplayDialog("提示", "打包AssetBundle结束", "确认");

                    SavePackageConfig();

                    return;
                }
            }


            if (EditorGUI.EndChangeCheck())
            {
                SavePackageConfig();
            }
        }
    }
}

