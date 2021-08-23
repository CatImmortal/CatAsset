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
        private List<BuildTarget> needDrawPlatforms = new List<BuildTarget>()
        {
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.Android,
            BuildTarget.iOS,
        };

        private Dictionary<BuildTarget, bool> selectedPlatform = new Dictionary<BuildTarget, bool>()
        {
            {BuildTarget.StandaloneWindows,false },
            {BuildTarget.StandaloneWindows64,false },
            {BuildTarget.Android,false},
            {BuildTarget.iOS,false},
        };

        private Dictionary<string, bool> selectedOptions = new Dictionary<string, bool>();

        private Dictionary<string, bool> abFoldOut = new Dictionary<string, bool>();

        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab = 0;
        private string[] tabs = { "打包设置", "资源预览" };


        private string[] options = Enum.GetNames(typeof(BuildAssetBundleOptions));

        private string outputPath = Directory.GetCurrentDirectory() + "/AssetBundleOutput";

        [MenuItem("CatAsset/打开打包窗口")]
        private static void OpenWindow()
        {
            PackageWindow window = GetWindow<PackageWindow>(false,"打包窗口");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            for (int i = 1; i < options.Length; i++)
            {
                selectedOptions[options[i]] = false;
            }

            List<AssetBundleBuild> abBuildList = new List<AssetBundleBuild>();
            PackageRuleConfig config = Util.GetPackageRuleConfig();
            foreach (PackageRule rule in config.Rules)
            {
                Func<string, AssetBundleBuild[]> func = AssetCollectFuncs.FuncDict[rule.Mode];
                AssetBundleBuild[] abBuilds = func(rule.Directory);
                abBuildList.AddRange(abBuilds);

            }


            foreach (AssetBundleBuild abBuild in abBuildList)
            {
                abFoldOut[abBuild.assetBundleName] = false;
            }
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
        /// 绘制打包设置界面
        /// </summary>
        private void DrawPackageConfig()
        {
            EditorGUILayout.LabelField("选择打包平台：");

            for (int i = 0; i < needDrawPlatforms.Count; i++)
            {
                using (EditorGUILayout.ToggleGroupScope toggle = new EditorGUILayout.ToggleGroupScope(needDrawPlatforms[i].ToString(), selectedPlatform[needDrawPlatforms[i]]))
                {
                    selectedPlatform[needDrawPlatforms[i]] = toggle.enabled;
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


            if (GUILayout.Button("打包AssetBundle", GUILayout.Width(200)))
            {
                //检查是否选中至少一个平台
                bool flag = false;
                foreach (KeyValuePair<BuildTarget, bool> item in selectedPlatform)
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
                foreach (KeyValuePair<BuildTarget, bool> item in selectedPlatform)
                {
                    if (item.Value == true)
                    {
                        Packager.BuildAssetBundle(outputPath, options, item.Key);
                    }
                }
            }
        }
        
        /// <summary>
        /// 绘制资源预览界面
        /// </summary>
        private void DrawAssetsPreview()
        {
            List<AssetBundleBuild> abBuildList = new List<AssetBundleBuild>();
            PackageRuleConfig config =  Util.GetPackageRuleConfig();
            foreach (PackageRule rule in config.Rules)
            {
                Func<string, AssetBundleBuild[]> func = AssetCollectFuncs.FuncDict[rule.Mode];
                AssetBundleBuild[] abBuilds = func(rule.Directory);
                abBuildList.AddRange(abBuilds);

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

