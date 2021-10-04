﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CatAsset;
using System.Reflection;
namespace CatAsset.Editor
{
    /// <summary>
    /// 资源运行时信息窗口
    /// </summary>
    public partial class AssetRuntimeInfoWindow : EditorWindow
    {
        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab;

        /// <summary>
        /// 页签
        /// </summary>
        private string[] tabs = { "资源信息", "任务信息","更新信息","资源组信息" };


        [MenuItem("CatAsset/打开资源运行时信息窗口", priority = 1)]
        private static void OpenWindow()
        {
            AssetRuntimeInfoWindow window = GetWindow<AssetRuntimeInfoWindow>(false, "资源运行时信息窗口");
            window.minSize = new Vector2(800, 600);
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
                    isInitAssetInfoView = false;
                    isInitTaskInfoView = false;
                    isInitUpdateInfoView = false;
                    isInitGroupInfoView = false;
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
                    DrawAssetInfoView();
                    break;

                case 1:
                    DrawTaskInfoView();
                    break;

                case 2:
                    DrawUpdateInfoView();
                    break;

                case 3:
                    DrawGroupInfoView();
                    break;
            }

            Repaint();
            
        }


        public static string GetByteDesc(long length)
        {
            int oneKB = 1024;
            int oneMB = 1024 * oneKB;
            int oneGB = 1024 * oneMB;

            if (length > oneGB)
            {
                return (length / (oneGB * 1f)).ToString("0.00") + "G" ;
            }
            if (length > oneMB)
            {
                return (length / (oneMB * 1f)).ToString("0.00") + "M";
            }
            if (length > oneKB)
            {
                return (length / (oneKB * 1f)).ToString("0.00") + "K";
            }

            return length + "B";
        }





    }
}

