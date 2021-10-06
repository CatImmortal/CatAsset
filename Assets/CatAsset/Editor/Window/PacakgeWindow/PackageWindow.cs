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
    public partial class PackageWindow : EditorWindow
    {



        /// <summary>
        /// 页签
        /// </summary>
        private string[] tabs = { "打包配置", "打包规则", "资源预览" };


        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab;


        [MenuItem("CatAsset/打开打包窗口", priority = 1)]
        private static void OpenWindow()
        {
            if (PkgUtil.PkgCfg == null || PkgUtil.PkgRuleCfg == null)
            {
                EditorUtility.DisplayDialog("提示", "需要先创建配置文件", "ok");
                return;
            }

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
                    DrawPackageConfigView();
                    break;
                case 1:
                    DrawPackageRuleView();
                    break;
                case 2:
                    DrawAssetsPreviewView();
                    break;
            }


        }
   
    

        
       

      
    }
}

