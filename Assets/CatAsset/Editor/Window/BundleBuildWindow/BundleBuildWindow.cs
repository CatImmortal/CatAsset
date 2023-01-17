using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.IMGUI.Controls;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建窗口
    /// </summary>
    public class BundleBuildWindow : EditorWindow
    {
        /// <summary>
        /// 页签
        /// </summary>
        private readonly string[] tabs = {"构建配置", "构建目录", "资源包预览"};
        
        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab;

        /// <summary>
        /// 子窗口列表
        /// </summary>
        private BaseSubWindow[] subWindows =
        {
            new ConfigWindow(),
            new BuildDirectoryWindow(),
            new PreviewWindow(),
        };

        /// <summary>
        /// 当前的子窗口
        /// </summary>
        private BaseSubWindow curSubWindow;
        
        private SearchField searchField;
        private string searchString;

        [MenuItem("CatAsset/打开资源包构建窗口", priority = 1)]
        private static void OpenWindow()
        {
            BundleBuildWindow window = GetWindow<BundleBuildWindow>(false, "资源包构建窗口");
            window.minSize = new Vector2(1100, 600);
            window.Show();
        }

        private void OnEnable()
        {
            //初始化子窗口
            foreach (var subWindow in subWindows)
            {
                subWindow.InitSubWindow();
            }
            curSubWindow = subWindows[0];
            searchField = new SearchField();
        }


        private void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            curSubWindow = subWindows[selectedTab];
            if (curSubWindow is BaseTreeViewSubWindow treeViewSubWindow)
            {
                DrawTreeViewToolBar(treeViewSubWindow);
                
            }
            curSubWindow.DrawSubWindow(position);
        }


        private void DrawTreeViewToolBar(BaseTreeViewSubWindow treeViewSubWindow)
        {
            float x = 0;
            float y = 30;
            float height = 20;
            float width = 0;
                
            x += width;
            width = 500;
            searchString = searchField.OnGUI(new Rect(x, y, width, height), searchString);
            treeViewSubWindow.TreeView.SearchString = searchString;

            x += width;
            x += 10;
            width = 50;
            if (GUI.Button(new Rect(x,y,width,height),"刷新"))
            {
                if (treeViewSubWindow is PreviewWindow)
                {
                    BundleBuildConfigSO.Instance.RefreshBundleBuildInfos();
                }
                
                ((TreeView)treeViewSubWindow.TreeView).Reload();
            }

            if (treeViewSubWindow is PreviewWindow)
            {
                x += width;
                x += 10;
                width = 100;
                if (GUI.Button(new Rect(x,y,width,height),"全部展开"))
                {
                    treeViewSubWindow.TreeView.ExpandAll();
                }
            
                x += width;
                x += 10;
                width = 100;
                if (GUI.Button(new Rect(x,y,width,height),"全部收起"))
                {
                    treeViewSubWindow.TreeView.CollapseAll();
                }
            
                x += width;
                x += 10;
                width = 150;
                if (GUI.Button(new Rect(x,y,width,height),"检测资源循环依赖"))
                {
                    LoopDependencyAnalyzer.AnalyzeAsset(BundleBuildConfigSO.Instance.Bundles);
                }
                
                x += width;
                x += 10;
                width = 150;
                if (GUI.Button(new Rect(x,y,width,height),"检测资源包循环依赖"))
                {
                    LoopDependencyAnalyzer.AnalyzeBundle(BundleBuildConfigSO.Instance.Bundles);
                }

                x += width;
                x += 10;
                width = 100;
                GUI.Label(new Rect(x,y,width,height),$"资源包数:{BundleBuildConfigSO.Instance.BundleCount}");
                
                x += width;
                x += 10;
                width = 100;
                GUI.Label(new Rect(x,y,width,height),$"资源数:{BundleBuildConfigSO.Instance.AssetCount}");
            }
            
           
            
        }





    }
}