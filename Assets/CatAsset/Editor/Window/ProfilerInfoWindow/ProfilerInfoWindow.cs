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
    public partial class ProfilerInfoWindow : EditorWindow
    {

        private ProfilerPlayer profilerPlayer = new ProfilerPlayer();

        /// <summary>
        /// 当前显示的分析器信息索引
        /// </summary>
        private int curProfilerInfoIndex = 0;

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

            //创建TreeView
            InitBundleInfoTreeView();
        }

        private void OnDisable()
        {
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

        /// <summary>
        /// 重新加载TreeView
        /// </summary>
        private void ReloadTreeView()
        {
            var curProfilerInfo = profilerPlayer.GetProfilerInfo(curProfilerInfoIndex);
            if (curProfilerInfo == null)
            {
                return;
            }

            bundleInfoTreeView.ProfilerInfo = curProfilerInfo;
            bundleInfoTreeView.Reload();
            bundleInfoTreeView.OnSortingChanged(bundleInfoTreeView.multiColumnHeader);
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
            float x = 0;
            float y = 30;
            float height = 20;
            float width = 0;

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            x += width;
            width = 500;
            searchString = searchField.OnGUI(new Rect(x, y, width, height), searchString);
            if (bundleInfoTreeView != null)
            {
                bundleInfoTreeView.searchString = searchString;
            }

            x += width;
            x += 10;
            width = 100;
            if (GUI.Button(new Rect(x,y,width,height),"全部展开"))
            {
                bundleInfoTreeView?.ExpandAll();
            }

            x += width;
            width = 100;
            if (GUI.Button(new Rect(x,y,width,height),"全部收起"))
            {
                bundleInfoTreeView?.CollapseAll();
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
            switch (selectedTab)
            {
                case 0:
                    DrawBundleInfoView();
                    break;

                case 1:
                    DrawTaskInfoView();
                    break;

                case 2:
                    DrawGroupInfoView();
                    break;

                case 3:
                    DrawUpdaterInfoView();
                    break;
            }
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





    }
}

