using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CatAsset;
using System.Reflection;
namespace CatAsset.Editor
{
    /// <summary>
    /// 运行时信息窗口
    /// </summary>
    public partial class RuntimeInfoWindow : EditorWindow
    {
        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab;

        /// <summary>
        /// 页签
        /// </summary>
        private string[] tabs = { "资源信息", "任务信息" ,"资源组信息" ,"更新器信息"};


        [MenuItem("CatAsset/打开运行时信息窗口", priority = 1)]
        private static void OpenWindow()
        {
            RuntimeInfoWindow window = GetWindow<RuntimeInfoWindow>(false, "运行时信息窗口");
            window.minSize = new Vector2(1200, 800);
            window.Show();
            
        }

        private void OnPlayModeChanged(PlayModeStateChange mode)
        {
            switch (mode)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    //窗口打开期间，每次运行游戏都重置下数据
                    isInitRuntimeInfoView = false;
                    isInitTaskInfoView = false;
                    isInitGroupInfoView = false;
                    isInitGroupUpdaterInfoView= false;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }


        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("运行后才能查看相关信息", MessageType.Warning);
                return;
            }

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            switch (selectedTab)
            {
                case 0:
                    DrawRuntimeInfoView();
                    break;

                case 1:
                    DrawTaskInfoView();
                    break;
                
                case 2:
                    DrawGroupInfoView();
                    break;
                
                case 3:
                    DrawGroupUpdaterInfoView();
                    break;
            }

            Repaint();
            
        }
        





    }
}

