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
            foreach (BuildTarget item in PkgUtil.PkgCfg.TargetPlatforms)
            {
                selectedPlatforms[item] = true;
            }

            for (int i = 1; i < options.Length; i++)
            {
                BuildAssetBundleOptions option = (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), options[i]);

                if ((PkgUtil.PkgCfg.Options & option) > 0)
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

            PkgUtil.PkgCfg.TargetPlatforms.Clear();
            foreach (KeyValuePair<BuildTarget, bool> item in selectedPlatforms)
            {
                if (item.Value == true)
                {
                    PkgUtil.PkgCfg.TargetPlatforms.Add(item.Key);
                }
            }

            PkgUtil.PkgCfg.Options = BuildAssetBundleOptions.None;
            foreach (KeyValuePair<string, bool> item in selectedOptions)
            {
                if (item.Value == true)
                {
                    BuildAssetBundleOptions option = (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), item.Key);
                    PkgUtil.PkgCfg.Options |= option;
                }
            }

            EditorUtility.SetDirty(PkgUtil.PkgCfg);
            AssetDatabase.SaveAssets();
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
                PkgUtil.PkgCfg.ManifestVersion = EditorGUILayout.IntField(PkgUtil.PkgCfg.ManifestVersion, GUILayout.Width(50));

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

            if (selectedOptions.Count == 0)
            {
                InitPackgeConfigView();
            }

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
                PkgUtil.PkgCfg.OutputPath = GUILayout.TextField(PkgUtil.PkgCfg.OutputPath);
                if (GUILayout.Button("选择目录", GUILayout.Width(100)))
                {
                    string folder = EditorUtility.OpenFolderPanel("选择打包输出根目录", PkgUtil.PkgCfg.OutputPath, "");
                    if (folder != string.Empty)
                    {
                        PkgUtil.PkgCfg.OutputPath = folder;
                    }
                }
            }

            EditorGUILayout.Space();

            using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope("冗余分析", PkgUtil.PkgCfg.IsAnalyzeRedundancy))
            {
                PkgUtil.PkgCfg.IsAnalyzeRedundancy = toggle.enabled;
            }


            EditorGUILayout.Space();

            using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope("打包平台只选中了1个时，打包后复制资源到StreamingAssets下", PkgUtil.PkgCfg.IsCopyToStreamingAssets))
            {
                PkgUtil.PkgCfg.IsCopyToStreamingAssets = toggle.enabled;
            }
            if (PkgUtil.PkgCfg.IsCopyToStreamingAssets)
            {
                EditorGUILayout.LabelField("要复制的资源组（以分号分隔，为空则全部复制）：");
                PkgUtil.PkgCfg.CopyGroup = EditorGUILayout.TextField(PkgUtil.PkgCfg.CopyGroup);
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {

                if (GUILayout.Button("打包AssetBundle", GUILayout.Width(200)))
                {

                    //检查是否选中至少一个平台
                    if (PkgUtil.PkgCfg.TargetPlatforms.Count == 0)
                    {
                        EditorUtility.DisplayDialog("提示", "至少要选择一个打包平台", "确认");
                        return;
                    }


                    //处理多个平台的打包
                    foreach (BuildTarget item in PkgUtil.PkgCfg.TargetPlatforms)
                    {
                        if (PkgUtil.PkgCfg.TargetPlatforms.Count == 1)
                        {
                            Packager.ExecutePackagePipeline(PkgUtil.PkgCfg.OutputPath, PkgUtil.PkgCfg.Options, item, PkgUtil.PkgCfg.ManifestVersion, PkgUtil.PkgCfg.IsAnalyzeRedundancy, PkgUtil.PkgCfg.IsCopyToStreamingAssets, PkgUtil.PkgCfg.CopyGroup);
                        }
                        else
                        {
                            Packager.ExecutePackagePipeline(PkgUtil.PkgCfg.OutputPath, PkgUtil.PkgCfg.Options, item, PkgUtil.PkgCfg.ManifestVersion, PkgUtil.PkgCfg.IsAnalyzeRedundancy, false,null);
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

