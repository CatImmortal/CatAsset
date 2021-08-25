using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 打包窗口
    /// </summary>
    public class PackageWindow : EditorWindow
    {
        /// <summary>
        /// 是否初始化过打包配置界面
        /// </summary>
        private bool isInitPackageConfigView;

        /// <summary>
        /// 是否初始化过资源预览界面
        /// </summary>
        private bool isInitAssetsPreviewView;


        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab;

        /// <summary>
        /// 页签
        /// </summary>
        private string[] tabs = { "打包配置", "资源预览" };

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
        /// 打包输出目录
        /// </summary>
        private string outputPath;

        /// <summary>
        /// 资源预览中的各assetbundle是否已展开
        /// </summary>
        private Dictionary<string, bool> abFoldOut = new Dictionary<string, bool>();

        /// <summary>
        /// 要打包的AssetBundleBuild列表
        /// </summary>
        private List<AssetBundleBuild> abBuildList;

        [MenuItem("CatAsset/打开打包窗口")]
        private static void OpenWindow()
        {
            PackageWindow window = GetWindow<PackageWindow>(false,"打包窗口");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }


        private void OnGUI()
        {

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            switch (selectedTab)
            {
                case 0:
                    DrawPackageConfig();
                    break;

                case 1:
                    DrawAssetsPreview();
                    break;
                default:
                    break;
            }


        }
   

        /// <summary>
        /// 初始化打包配置界面
        /// </summary>
        private void InitPackgeConfigView()
        {
            foreach (BuildTarget item in Util.PkgCfg.targetPlatforms)
            {
                selectedPlatforms[item] = true;
            }



            for (int i = 1; i < options.Length; i++)
            {
                BuildAssetBundleOptions option = (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), options[i]);

                if ((Util.PkgCfg.options & option) > 0)
                {
                    selectedOptions[options[i]] = true;
                }
                else
                {
                    selectedOptions[options[i]] = false;
                }
            }

            outputPath = Util.PkgCfg.outputPath;
        }

        /// <summary>
        /// 绘制打包配置界面
        /// </summary>
        private void DrawPackageConfig()
        {
            if (!isInitPackageConfigView)
            {
                InitPackgeConfigView();
                isInitPackageConfigView = true;
            }

            EditorGUILayout.LabelField("选择打包平台：");

            for (int i = 0; i < targetPlatforms.Count; i++)
            {
                using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope(targetPlatforms[i].ToString(), selectedPlatforms[targetPlatforms[i]]))
                {
                    selectedPlatforms[targetPlatforms[i]] = toggle.enabled;
                }

            }



            EditorGUILayout.Space();

            EditorGUILayout.LabelField("选择打包设置：");
         

            for (int i = 1; i < options.Length; i++)
            {
                using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope(options[i], selectedOptions[options[i]]))
                {
                    selectedOptions[options[i]] = toggle.enabled;
                }

            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("打包输出目录：", GUILayout.Width(100));
                outputPath = GUILayout.TextField(outputPath);
            }

            if (GUILayout.Button("保存打包配置", GUILayout.Width(200)))
            {
                Util.PkgCfg.targetPlatforms.Clear();
                foreach (KeyValuePair<BuildTarget, bool> item in selectedPlatforms)
                {
                    if (item.Value == true)
                    {
                        Util.PkgCfg.targetPlatforms.Add(item.Key);
                    }
                }

                Util.PkgCfg.options = BuildAssetBundleOptions.None;
                foreach (KeyValuePair<string, bool> item in selectedOptions)
                {
                    if (item.Value == true)
                    {
                        BuildAssetBundleOptions option = (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), item.Key);
                        Util.PkgCfg.options |= option;
                    }
                }

                Util.PkgCfg.outputPath = outputPath;
                EditorUtility.DisplayDialog("提示", "保存完毕", "确认");
            }

            if (GUILayout.Button("打包AssetBundle", GUILayout.Width(200)))
            {
                //检查是否选中至少一个平台
                bool flag = false;
                foreach (KeyValuePair<BuildTarget, bool> item in selectedPlatforms)
                {
                    if (item.Value == true)
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag == false)
                {
                    EditorUtility.DisplayDialog("提示", "至少要选择一个打包平台", "确认");
                    return;
                }

                //处理打包设置
                BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
                foreach (KeyValuePair<string, bool> item in selectedOptions)
                {
                    if (item.Value == true)
                    {
                        options |= (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), item.Key);
                    }
                }

                //处理多个平台
                foreach (KeyValuePair<BuildTarget, bool> item in selectedPlatforms)
                {
                    if (item.Value == true)
                    {
                        Packager.ExecutePackagePipeline(outputPath, options, item.Key);
                    }
                }

                EditorUtility.DisplayDialog("提示", "打包AssetBundle结束", "确认");
             
            }
        
            
        }
        
        /// <summary>
        /// 初始化资源预览界面
        /// </summary>
        private void InitAssetsPreviewView()
        {
            abBuildList = Util.PkgRuleCfg.GetAssetBundleBuildList();
            foreach (AssetBundleBuild abBuild in abBuildList)
            {
                abFoldOut[abBuild.assetBundleName] = false;
            }
        }

        /// <summary>
        /// 绘制资源预览界面
        /// </summary>
        private void DrawAssetsPreview()
        {
            if (!isInitAssetsPreviewView)
            {
                InitAssetsPreviewView();
                isInitAssetsPreviewView = true;
            }

            foreach (AssetBundleBuild abBuild in abBuildList)
            {
                abFoldOut[abBuild.assetBundleName] = EditorGUILayout.Foldout(abFoldOut[abBuild.assetBundleName],abBuild.assetBundleName);

                if (abFoldOut[abBuild.assetBundleName] == true)
                {
                    foreach (string assetName in abBuild.assetNames)
                    {
                        EditorGUILayout.LabelField("\t" + assetName);
                    }
                }
            }
        }
    }
}

