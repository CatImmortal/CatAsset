using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建窗口
    /// </summary>
    public partial class BundleBuildWindow : EditorWindow
    {
        /// <summary>
        /// 页签
        /// </summary>
        private readonly string[] tabs = {"构建配置", "构建目录", "资源包预览"};
        
        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab;

        private BundleBuildConfigSO bundleBuildConfg;

        [MenuItem("CatAsset/打开资源包构建窗口", priority = 1)]
        private static void OpenWindow()
        {
            BundleBuildWindow window = GetWindow<BundleBuildWindow>(false, "资源包构建窗口");
            window.minSize = new Vector2(800, 600);
            window.bundleBuildConfg = Util.GetConfigAsset<BundleBuildConfigSO>();
            if (window.bundleBuildConfg == null)
            {
                EditorUtility.DisplayDialog("提示", "需要先创建BundleBuildConfig文件", "ok");
            }
            else
            {
                window.Show();
            }

        }



        private void OnGUI()
        {

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            switch (selectedTab)
            {
                case 0:
                    //DrawPackageConfigView();
                    break;
                case 1:
                    DrawBundleBuildDirectoryView();
                    break;
                case 2:
                    DrawBundlePreviewView();
                    break;
            }


        }







    }
}