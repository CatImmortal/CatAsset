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
        /// 资源清单版本
        /// </summary>
        private int manifestVersion;

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
        /// 是否进行冗余分析
        /// </summary>
        private bool isAnalyzeRedundancy;

        /// <summary>
        /// 打包输出目录
        /// </summary>
        private string outputPath;

        /// <summary>
        /// 打包平台只有1个时，打包后是否将资源复制到StreamingAssets目录下
        /// </summary>
        private bool isCopyToStreamingAssets;

        /// <summary>
        /// 初始化打包配置界面
        /// </summary>
        private void InitPackgeConfigView()
        {
            manifestVersion = Util.PkgCfg.ManifestVersion;
            isCopyToStreamingAssets = Util.PkgCfg.IsCopyToStreamingAssets;
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

            isAnalyzeRedundancy = Util.PkgCfg.IsAnalyzeRedundancy;
            outputPath = Util.PkgCfg.OutputPath;
        }

        /// <summary>
        /// 保存打包配置
        /// </summary>
        private void SavePackageConfig()
        {
            Util.PkgCfg.ManifestVersion = manifestVersion;

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

            Util.PkgCfg.IsAnalyzeRedundancy = isAnalyzeRedundancy;
            Util.PkgCfg.OutputPath = outputPath;

            EditorUtility.SetDirty(Util.PkgCfg);
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

            EditorGUILayout.LabelField("游戏版本号：" + Application.version);
            manifestVersion = EditorGUILayout.IntField("资源清单版本号：", manifestVersion, GUILayout.Width(200));

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
                outputPath = GUILayout.TextField(outputPath);
                if (GUILayout.Button("选择目录", GUILayout.Width(100)))
                {
                    string folder = EditorUtility.OpenFolderPanel("选择打包输出根目录", outputPath, "");
                    if (folder != string.Empty)
                    {
                        outputPath = folder;
                    }
                }
            }

            EditorGUILayout.Space();

            using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope("冗余分析", isAnalyzeRedundancy))
            {
                isAnalyzeRedundancy = toggle.enabled;
            }

            EditorGUILayout.Space();

            using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope("打包平台只选中了1个时，打包后复制资源到StreamingAssets下", isCopyToStreamingAssets))
            {
                isCopyToStreamingAssets = toggle.enabled;
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("保存打包配置", GUILayout.Width(200)))
                {
                    SavePackageConfig();
                    EditorUtility.DisplayDialog("提示", "保存完毕", "确认");
                }

                if (GUILayout.Button("打包AssetBundle", GUILayout.Width(200)))
                {
                    //保存打包配置
                    SavePackageConfig();

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
                            Packager.ExecutePackagePipeline(outputPath, Util.PkgCfg.Options, item, Util.PkgCfg.ManifestVersion, isCopyToStreamingAssets, isAnalyzeRedundancy);
                        }
                        else
                        {
                            Packager.ExecutePackagePipeline(outputPath, Util.PkgCfg.Options, item, Util.PkgCfg.ManifestVersion, false, isAnalyzeRedundancy);
                        }
                    }
                    EditorUtility.SetDirty(Util.PkgCfg);
                    AssetDatabase.SaveAssets();

                    //修改窗口上显示的资源清单版本号
                    manifestVersion = Util.PkgCfg.ManifestVersion;


                    EditorUtility.DisplayDialog("提示", "打包AssetBundle结束", "确认");

                }
            }




        }
    }
}

