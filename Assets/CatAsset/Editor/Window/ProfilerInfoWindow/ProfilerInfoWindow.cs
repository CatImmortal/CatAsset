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
        /// 选择的页签
        /// </summary>
        private int selectedTab;

        /// <summary>
        /// 页签
        /// </summary>
        private string[] tabs = { "资源包信息", "任务信息" ,"资源组信息" ,"更新器信息"};


        [MenuItem("CatAsset/打开调试分析器窗口", priority = 1)]
        private static void OpenWindow()
        {
            ProfilerInfoWindow window = GetWindow<ProfilerInfoWindow>(false, "调试分析器窗口");
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
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    ClearRuntimeInfoView();
                    break;
                default:
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

            ProfilerInfo profilerInfo = ProfilerInfo.Deserialize(args.data);
            OnProfilerInfo(args.playerId,profilerInfo);
        }

        /// <summary>
        /// 接收分析器信息的回调
        /// </summary>
        private void OnProfilerInfo(int id, ProfilerInfo profilerInfo)
        {
            switch (profilerInfo.Type)
            {
                case ProfilerInfoType.None:
                    break;
                case ProfilerInfoType.Bundle:
                    bundleInfoList = profilerInfo.BundleInfoList;
                    break;
                case ProfilerInfoType.Task:
                    break;
                case ProfilerInfoType.Group:
                    break;
                case ProfilerInfoType.Updater:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            Send(ProfilerInfoType.None);
        }


        private void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            switch (selectedTab)
            {
                case 0:
                    Send(ProfilerInfoType.Bundle);
                    DrawBundleInfoView();
                    break;

                case 1:
                    Send(ProfilerInfoType.Task);
                    DrawTaskInfoView();
                    break;

                case 2:
                    Send(ProfilerInfoType.Group);
                    DrawGroupInfoView();
                    break;

                case 3:
                    Send(ProfilerInfoType.Updater);
                    DrawGroupUpdaterInfoView();
                    break;
            }

            Repaint();

        }

        /// <summary>
        /// 向真机发送消息
        /// </summary>
        private void Send(ProfilerInfoType type)
        {
            ProfilerComponent.CurType = type;
            EditorConnection.instance.Send(ProfilerComponent.MsgSendEditorToPlayer,
                BitConverter.GetBytes((int)type));
        }





    }
}

