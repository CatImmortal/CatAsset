using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using CatAsset;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using CatAsset.Runtime;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;

namespace CatAsset.Editor
{
    /// <summary>
    /// 调试分析器信息窗口
    /// </summary>
    public partial class ProfilerInfoWindow : EditorWindow
    {
        /// <summary>
        /// 分析器信息
        /// </summary>
        private ProfilerInfo profilerInfo;

        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab;

        /// <summary>
        /// 页签
        /// </summary>
        private string[] tabs = { "资源包信息", "任务信息" ,"资源组信息" ,"更新器信息"};

        private SearchField searchField;
        private string searchString;

        [MenuItem("CatAsset/打开调试分析器窗口", priority = 1)]
        private static void OpenWindow()
        {
            ProfilerInfoWindow window = GetWindow<ProfilerInfoWindow>(false, "调试分析器窗口");
            window.minSize = new Vector2(1600, 800);
            window.Show();

        }

        private void OnPlayModeChanged(PlayModeStateChange mode)
        {
            switch (mode)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    profilerInfo = null;
                    break;
            }
        }

        private void OnEnable()
        {
            EditorConnection.instance.Initialize();
            EditorConnection.instance.RegisterConnection(OnConnection);
            EditorConnection.instance.RegisterDisconnection(OnDisconnection);
            EditorConnection.instance.Register(ProfilerComponent.MsgSendPlayerToEditor, OnPlayerMessage);

            ProfilerComponent.Callback = OnProfilerInfo;

            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            searchField = new SearchField();
            //m_SearchField.downOrUpArrowKeyPressed += bundleInfoTreeView.SetFocusAndEnsureSelectedItem;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            Send(false);
        }

        private void OnConnection(int arg0)
        {
            Debug.Log($"真机调试已连接：{arg0}");
        }

        private void OnDisconnection(int arg0)
        {
            Debug.Log($"真机调试已断开：{arg0}");
        }


        /// <summary>
        /// 接收真机消息的回调
        /// </summary>
        private void OnPlayerMessage(MessageEventArgs args)
        {
            if (Application.isPlaying)
            {
                return;
            }

            profilerInfo = ProfilerInfo.Deserialize(args.data);
        }

        /// <summary>
        /// 编辑器下接收分析器信息的回调
        /// </summary>
        private void OnProfilerInfo(int id, ProfilerInfo info)
        {
            if (profilerInfo != null)
            {
                ReferencePool.Release(profilerInfo);
            }

            profilerInfo = info;

            if (bundleInfoTreeView == null)
            {
                InitBundleInfoTreeView();
            }
            bundleInfoTreeView.ProfilerInfo = profilerInfo;
            if (profilerInfo.BundleInfoList.Count > 0)
            {
                bundleInfoTreeView.Reload();
                bundleInfoTreeView.OnSortingChanged(bundleInfoTreeView.multiColumnHeader);
            }

        }

        /// <summary>
        /// 创建列的数组
        /// </summary>
        private MultiColumnHeaderState.Column[] CreateColumns(List<string> columnList)
        {
            var columns = new MultiColumnHeaderState.Column[columnList.Count];
            for (int i = 0; i < columns.Length; i++)
            {
                string name = columnList[i];
                columns[i] = new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent(name),
                    headerTextAlignment = TextAlignment.Center,
                    sortingArrowAlignment = TextAlignment.Right
                };
            }

            return columns;
        }


        private void OnGUI()
        {
            DrawUpToolbar();
            DrawInfoView();
            //Repaint();
        }

        /// <summary>
        /// 绘制上方工具栏
        /// </summary>
        private void DrawUpToolbar()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            searchString = searchField.OnGUI(new Rect(0, 30, 500, 20), searchString);
            if (bundleInfoTreeView != null)
            {
                bundleInfoTreeView.searchString = searchString;
            }

            if (GUI.Button(new Rect(510,30,100,20),"全部展开"))
            {
                bundleInfoTreeView.ExpandAll();
            }
            if (GUI.Button(new Rect(610,30,100,20),"全部收起"))
            {
                bundleInfoTreeView.CollapseAll();
            }
        }

        /// <summary>
        /// 绘制信息界面
        /// </summary>
        private void DrawInfoView()
        {
            switch (selectedTab)
            {
                case 0:
                    Send(true);
                    DrawBundleInfoView();
                    break;

                case 1:
                    Send(true);
                    DrawTaskInfoView();
                    break;

                case 2:
                    Send(true);
                    DrawGroupInfoView();
                    break;

                case 3:
                    Send(true);
                    DrawUpdaterInfoView();
                    break;
            }
        }



        /// <summary>
        /// 向真机发送消息
        /// </summary>
        private void Send(bool isOpen)
        {
            ProfilerComponent.IsOpen = isOpen;
            EditorConnection.instance.Send(ProfilerComponent.MsgSendEditorToPlayer,
                BitConverter.GetBytes(isOpen));
        }





    }
}

