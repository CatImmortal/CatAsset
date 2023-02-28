using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using CatAsset.Runtime;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;
using Debug = UnityEngine.Debug;

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
        private string[] tabs = { "资源包信息", "资源信息", "任务信息" ,"资源组信息" ,"更新器信息","对象池信息"};

        /// <summary>
        /// 子窗口列表
        /// </summary>
        private BaseTreeViewSubWindow[] subWindows =
        {
            new BundleInfoWindow(),
            new AssetInfoWindow(),
            new TaskInfoWindow(),
            new GroupInfoWindow(),
            new UpdaterInfoWindow(),
            new PoolInfoWindow(),
        };

        /// <summary>
        /// 当前的子窗口
        /// </summary>
        private BaseTreeViewSubWindow curSubWindow;

        private SearchField searchField;
        private string searchString;

        /// <summary>
        /// 是否开启连续采样
        /// </summary>
        private bool isSampleMulti;
        private float sampleInterval = 1 / 15f; //连续采样频率为15帧
        private static Stopwatch sw = new Stopwatch();
        
        [MenuItem("CatAsset/打开调试分析器窗口", priority = 1)]
        private static void OpenWindow()
        {
            ProfilerInfoWindow window = GetWindow<ProfilerInfoWindow>(false, "调试分析器窗口");
            window.minSize = new Vector2(1600, 800);
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
            EditorApplication.update -= EditorUpdate;
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
                    if (isSampleMulti)
                    {
                        //编辑器下启动游戏后 已注册的EditorUpdate 会被清理掉 如果是开启连续采样下启动的游戏 得重新注册
                        EditorApplication.update += EditorUpdate;
                        sw.Start();
                    }
                    break;

                case PlayModeStateChange.ExitingPlayMode:
 
                    break;
            }
        }

        private void OnGUI()
        {
            DrawUpToolbar();
            DrawSubWindow();
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
            curSubWindow.TreeView.SearchString = searchString;

            x += width;
            x += 10;
            width = 100;
            if (GUI.Button(new Rect(x,y,width,height),"全部展开"))
            {
                curSubWindow.TreeView.ExpandAll();
            }

            x += width;
            width = 100;
            if (GUI.Button(new Rect(x,y,width,height),"全部收起"))
            {
                curSubWindow.TreeView.CollapseAll();
            }

            x += width;
            x += 20;
            width = 100;
            if (GUI.Button(new Rect(x, y, width, height),"单次采样"))
            {
                Send(ProfilerMessageType.SampleOnce);
            }
            
            x += width;
            width = 100;

            string btnText = "开始连续采样";
            if (isSampleMulti)
            {
                btnText = "停止连续采样";
            }

            if (GUI.Button(new Rect(x, y, width, height),btnText))
            {
                isSampleMulti = !isSampleMulti;

                if (isSampleMulti)
                {
                    EditorApplication.update += EditorUpdate;
                    sw.Start();
                }
                else
                {
                    EditorApplication.update -= EditorUpdate;
                    sw.Reset();
                }
            }

            x += width;
            width = 100;
            if (GUI.Button(new Rect(x, y, width, height),"清空"))
            {
                profilerPlayer.ClearProfilerInfo();
                curProfilerInfoIndex = 0;
                foreach (var subWindow in subWindows)
                {
                    subWindow.TreeView.Reload(null);
                }
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
        /// 绘制子窗口
        /// </summary>
        private void DrawSubWindow()
        {
            curSubWindow = subWindows[selectedTab];
            curSubWindow.DrawSubWindow(position);
        }

        private void EditorUpdate()
        {
            if (EditorApplication.isPaused)
            {
                //编辑器运行游戏后的暂停状态 停止连续采样
                return;
            }
            
            if (sw.Elapsed.TotalSeconds >= sampleInterval)
            {
                Send(ProfilerMessageType.SampleOnce);
                sw.Restart();
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

            foreach (var subWindow in subWindows)
            {
                subWindow.TreeView.Reload(info);
            }
        }



    }
}

