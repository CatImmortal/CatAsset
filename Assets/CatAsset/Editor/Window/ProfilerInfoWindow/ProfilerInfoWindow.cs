using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CatAsset.Runtime;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;

namespace CatAsset.Editor
{
    /// <summary>
    /// 调试分析器信息窗口
    /// </summary>
    public class ProfilerInfoWindow : EditorWindow
    {
        private ProfilerPlayer profilerPlayer = new ProfilerPlayer();

        /// <summary>
        /// 当前显示的分析器信息索引
        /// </summary>
        private int curProfilerInfoIndex;

        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab;

        /// <summary>
        /// 页签
        /// </summary>
        private string[] tabs = { "资源包信息", "资源信息", "任务信息" ,"资源组信息" ,"更新器信息"};

        /// <summary>
        /// 分析器信息界面
        /// </summary>
        private BaseProfilerInfoView[] infoViews =
        {
            new BundleInfoView(),
            new AssetInfoView(),
            new TaskInfoView(),
            new GroupInfoView(),
            new UpdaterInfoView()
        };

        /// <summary>
        /// 当前的信息界面
        /// </summary>
        private BaseProfilerInfoView curInfoView;

        private SearchField searchField;
        private string searchString;

        [MenuItem("CatAsset/打开调试分析器窗口", priority = 1)]
        private static void OpenWindow()
        {
            ProfilerInfoWindow window = GetWindow<ProfilerInfoWindow>(false, "调试分析器窗口");
            window.minSize = new Vector2(1600, 800);
            window.Show();

        }

        private void OnEnable()
        {
            //初始化TreeView
            foreach (var infoView in infoViews)
            {
                infoView.InitTreeView();
            }
            curInfoView = infoViews[0];
            searchField = new SearchField();

            EditorConnection.instance.Initialize();
            EditorConnection.instance.RegisterConnection(OnConnection);
            EditorConnection.instance.RegisterDisconnection(OnDisconnection);
            EditorConnection.instance.Register(ProfilerComponent.MsgSendPlayerToEditor, OnPlayerMessage);

            ProfilerComponent.Callback = OnProfilerInfo;

            EditorApplication.playModeStateChanged += OnPlayModeChanged;


        }

        private void OnDisable()
        {
            EditorConnection.instance.UnregisterConnection(OnConnection);
            EditorConnection.instance.UnregisterDisconnection(OnDisconnection);
            EditorConnection.instance.Unregister(ProfilerComponent.MsgSendPlayerToEditor,OnPlayerMessage);

            ProfilerComponent.Callback = null;

            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

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

            var info = ProfilerInfo.Deserialize(args.data);
            OnProfilerInfo(args.playerId, info);
        }

        /// <summary>
        /// 编辑器下接收分析器信息的回调
        /// </summary>
        private void OnProfilerInfo(int id, ProfilerInfo info)
        {
            profilerPlayer.AddProfilerInfo(info);

            curProfilerInfoIndex = profilerPlayer.MaxRange;
            ReloadTreeView();
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
                    curProfilerInfoIndex = 0;
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void OnGUI()
        {
            DrawUpToolbar();
            DrawInfoView();
        }

        /// <summary>
        /// 绘制上方工具栏
        /// </summary>
        private void DrawUpToolbar()
        {
            float x = 0;
            float y = 30;
            float height = 20;
            float width = 0;

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            x += width;
            width = 500;
            searchString = searchField.OnGUI(new Rect(x, y, width, height), searchString);
            curInfoView.TreeView.searchString = searchString;

            x += width;
            x += 10;
            width = 100;
            if (GUI.Button(new Rect(x,y,width,height),"全部展开"))
            {
                curInfoView.TreeView.ExpandAll();
            }

            x += width;
            width = 100;
            if (GUI.Button(new Rect(x,y,width,height),"全部收起"))
            {
                curInfoView.TreeView.CollapseAll();
            }

            x += width;
            x += 20;
            width = 100;
            if (GUI.Button(new Rect(x, y, width, height),"采样"))
            {
                Send(ProfilerMessageType.SampleOnce);
            }

            x += width;
            width = 100;
            if (GUI.Button(new Rect(x, y, width, height),"清空"))
            {
                profilerPlayer.ClearProfilerInfo();
                curProfilerInfoIndex = 0;
            }

            //绘制Slider
            x += width;
            x += 10;
            width = 300;
            var sliderRect = new Rect(x, y, width, height);
            int sliderIndex = EditorGUI.IntSlider(sliderRect, curProfilerInfoIndex, 0, Mathf.Max(0,profilerPlayer.MaxRange));
            x += width;
            width = 50;
            EditorGUI.LabelField(new Rect(x, y, width, height),$" / {Mathf.Max(0,profilerPlayer.MaxRange)}");

            //上一帧
            x += width;
            width = 20;
            if (sliderIndex > 0 && GUI.Button(new Rect(x, y, width, height),"<"))
            {
                sliderIndex--;
            }

            //下一帧
            x += width;
            x += 5;
            width = 20;
            if (sliderIndex < profilerPlayer.MaxRange && GUI.Button(new Rect(x, y, width, height),">"))
            {
                sliderIndex++;
            }

            if (sliderIndex != curProfilerInfoIndex)
            {
                curProfilerInfoIndex = sliderIndex;
                ReloadTreeView();
            }
        }

        /// <summary>
        /// 绘制信息界面
        /// </summary>
        private void DrawInfoView()
        {
            curInfoView = infoViews[selectedTab];
            curInfoView.DrawInfoView(position);
        }

        /// <summary>
        /// 向真机发送消息
        /// </summary>
        private void Send(ProfilerMessageType msgType)
        {
            ProfilerComponent.OnEditorMessage(msgType);
            EditorConnection.instance.Send(ProfilerComponent.MsgSendEditorToPlayer,
                BitConverter.GetBytes((int)msgType));
        }

        /// <summary>
        /// 重新加载TreeView
        /// </summary>
        private void ReloadTreeView()
        {
            ProfilerInfo info = profilerPlayer.GetProfilerInfo(curProfilerInfoIndex);
            if (info == null)
            {
                return;
            }

            foreach (var infoView in infoViews)
            {
                infoView.TreeView.Reload(info);
            }
        }




    }
}

