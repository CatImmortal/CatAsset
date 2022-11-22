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
            EditorConnection.instance.Register(CatAssetProfilerComponent.MsgSendPlayerToEditor, OnPlayerMessage);

            CatAssetProfilerComponent.Callback = OnProfilerInfo;

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



        private void OnPlayerMessage(MessageEventArgs args)
        {
            if (Application.isPlaying)
            {
                return;
            }

            Debug.Log($"接收到真机调试数据，length == {args.data.Length}");

            ProfilerInfo profilerInfo = ProfilerInfo.Deserialize(args.data);
            OnProfilerInfo(args.playerId,profilerInfo);
        }

        private void OnProfilerInfo(int id, ProfilerInfo profilerInfo)
        {
            switch (profilerInfo.Type)
            {
                case ProfilerInfoType.None:
                    break;
                case ProfilerInfoType.Bundle:
                    bundleInfo = profilerInfo.BundleInfo;
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
            // if (!Application.isPlaying)
            // {
            //     EditorGUILayout.HelpBox("运行后才能查看相关信息", MessageType.Warning);
            //     return;
            // }

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            switch (selectedTab)
            {
                case 0:
                    Send(ProfilerInfoType.Bundle);
                    DrawRuntimeInfoView();
                    break;

                case 1:
                    //Send(ProfilerDataType.Task);
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

        private void Send(ProfilerInfoType type)
        {
            CatAssetProfilerComponent.CurType = type;
            EditorConnection.instance.Send(CatAssetProfilerComponent.MsgSendEditorToPlayer,
                BitConverter.GetBytes((int)type));
        }





    }
}

