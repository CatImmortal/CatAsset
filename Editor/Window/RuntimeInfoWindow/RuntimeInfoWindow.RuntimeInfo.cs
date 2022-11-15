using System;
using System.Collections.Generic;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class RuntimeInfoWindow
    {
        private bool isInitRuntimeInfoView;

        private Dictionary<string, BundleRuntimeInfo> bundleRuntimeInfoDict;
        private Dictionary<string, AssetRuntimeInfo> assetRuntimeInfoDict;

        private Vector2 scrollPos;

        /// <summary>
        /// 是否只显示主动加载的资源
        /// </summary>
        private bool isOnlyShowActiveLoad = false;

        /// <summary>
        /// 资源包相对路径->是否展开
        /// </summary>
        private Dictionary<string, bool> foldOutDict = new Dictionary<string, bool>();

        /// <summary>
        /// 初始化运行时信息界面
        /// </summary>
        private void InitRuntimeInfoView()
        {
            isInitRuntimeInfoView = true;

            bundleRuntimeInfoDict = typeof(CatAssetDatabase).GetField(nameof(bundleRuntimeInfoDict), BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, BundleRuntimeInfo>;
            assetRuntimeInfoDict = typeof(CatAssetDatabase).GetField(nameof(assetRuntimeInfoDict), BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, AssetRuntimeInfo>;

        }

        /// <summary>
        /// 绘制运行时信息界面
        /// </summary>
        private void DrawRuntimeInfoView()
        {
            if (!isInitRuntimeInfoView)
            {
                InitRuntimeInfoView();
            }

            bool isAllFoldOutTrue = false;
            bool isAllFoldOutFalse = false;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("全部展开", GUILayout.Width(100)))
                {
                    isAllFoldOutTrue = true;
                }

                if (GUILayout.Button("全部收起", GUILayout.Width(100)))
                {
                    isAllFoldOutFalse = true;
                }

                isOnlyShowActiveLoad = GUILayout.Toggle(isOnlyShowActiveLoad, "只显示主动加载的资源", GUILayout.Width(200));
            }

            using (EditorGUILayout.ScrollViewScope sv = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = sv.scrollPosition;

                foreach (KeyValuePair<string, BundleRuntimeInfo> item in bundleRuntimeInfoDict)
                {


                    string bundleRelativePath = item.Key;
                    BundleRuntimeInfo bundleRuntimeInfo = item.Value;

                    if (!foldOutDict.ContainsKey(bundleRelativePath))
                    {
                        foldOutDict.Add(bundleRelativePath, false);
                    }

                    if (isAllFoldOutTrue)
                    {
                        //点击过全部展开
                        foldOutDict[bundleRelativePath] = true;
                    }
                    else if (isAllFoldOutFalse)
                    {
                        //点击过全部收起
                        foldOutDict[bundleRelativePath] = false;
                    }


                    //没有资源在使用 也没下游资源包 不显示
                    if (bundleRuntimeInfo.UsingAssets.Count == 0 && bundleRuntimeInfo.DependencyChain.DownStream.Count == 0)
                    {
                        continue;
                    }

                    if (isOnlyShowActiveLoad)
                    {
                        //仅显示主动加载的资源
                        //此资源包至少有一个主动加载的资源，才能显示
                        bool canShow = false;
                        foreach (AssetRuntimeInfo assetRuntimeInfo in bundleRuntimeInfo.UsingAssets)
                        {
                            if (assetRuntimeInfo.RefCount != assetRuntimeInfo.DependencyChain.DownStream.Count)
                            {
                                canShow = true;
                                break;
                            }
                        }
                        if (!canShow)
                        {
                            continue;
                        }
                    }




                    using (new EditorGUILayout.HorizontalScope())
                    {
                        foldOutDict[bundleRelativePath] = EditorGUILayout.Foldout(foldOutDict[bundleRelativePath], bundleRelativePath);

                        DrawBundleRuntimeInfo(bundleRuntimeInfo,false);
                    }

                    if (foldOutDict[bundleRelativePath])
                    {
                        EditorGUILayout.Space();

                        foreach (AssetRuntimeInfo assetRuntimeInfo in bundleRuntimeInfo.UsingAssets)
                        {
                            if (isOnlyShowActiveLoad && assetRuntimeInfo.RefCount == assetRuntimeInfo.DependencyChain.DownStream.Count)
                            {
                                //只显示主动加载的资源 且此资源纯被依赖加载的 就跳过
                                continue;
                            }

                            DrawAssetRuntimeInfo(assetRuntimeInfo);
                        }

                        EditorGUILayout.Space();
                    }
                }
            }


        }

        /// <summary>
        /// 绘制资源包运行时信息
        /// </summary>
        private void DrawBundleRuntimeInfo(BundleRuntimeInfo bundleRuntimeInfo,bool isDrawName = true)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (isDrawName)
                {
                    EditorGUILayout.LabelField($"{bundleRuntimeInfo.Manifest.RelativePath}" ,GUILayout.Width(400));
                }
                else
                {
                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField($"资源组：{bundleRuntimeInfo.Manifest.Group}" ,GUILayout.Width(150));
                EditorGUILayout.LabelField($"使用中资源数：{bundleRuntimeInfo.UsingAssets.Count}/{bundleRuntimeInfo.Manifest.Assets.Count}" ,GUILayout.Width(125));
                EditorGUILayout.LabelField($"文件长度：{RuntimeUtil.GetByteLengthDesc(bundleRuntimeInfo.Manifest.Length)}",GUILayout.Width(125));

                EditorGUILayout.LabelField($"上游资源包数量：{bundleRuntimeInfo.DependencyChain.UpStream.Count}",GUILayout.Width(125));
                if (GUILayout.Button("查看上游资源包", GUILayout.Width(100)))
                {
                    if (bundleRuntimeInfo.DependencyChain.UpStream.Count > 0)
                    {
                        BundleListWindow.OpenWindow(this,bundleRuntimeInfo.DependencyChain.UpStream);
                    }
                }

                EditorGUILayout.LabelField($"下游资源包数量：{bundleRuntimeInfo.DependencyChain.DownStream.Count}",GUILayout.Width(125));

                if (GUILayout.Button("查看下游资源包", GUILayout.Width(100)))
                {
                    if (bundleRuntimeInfo.DependencyChain.DownStream.Count > 0)
                    {
                        BundleListWindow.OpenWindow(this,bundleRuntimeInfo.DependencyChain.DownStream);
                    }
                }
                EditorGUILayout.LabelField("", GUILayout.Width(30));
            }


        }

        /// <summary>
        /// 绘制资源运行时信息
        /// </summary>
        private void DrawAssetRuntimeInfo(AssetRuntimeInfo assetRuntimeInfo)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                //资源名
                EditorGUILayout.LabelField("\t" + assetRuntimeInfo.AssetManifest.Name);

                //对象引用
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetRuntimeInfo.AssetManifest.Name), typeof(UnityEngine.Object),false);
                EditorGUI.EndDisabledGroup();

                //长度
                EditorGUILayout.LabelField($"\t长度：{RuntimeUtil.GetByteLengthDesc(assetRuntimeInfo.AssetManifest.Length)}");

                //引用计数
                EditorGUILayout.LabelField($"\t引用计数：{assetRuntimeInfo.RefCount}");

                //上游资源数
                EditorGUILayout.LabelField($"\t上游资源数量：{assetRuntimeInfo.DependencyChain.UpStream.Count}");

                //下游资源数
                EditorGUILayout.LabelField($"\t下游资源数量：{assetRuntimeInfo.DependencyChain.DownStream.Count}");

                if (GUILayout.Button("查看资源依赖关系图") && (assetRuntimeInfo.DependencyChain.UpStream.Count > 0 || assetRuntimeInfo.DependencyChain.DownStream.Count > 0))
                {
                    DependencyGraphViewWindow.Open<AssetRuntimeInfo,AssetNode>(assetRuntimeInfo);
                }
            }
        }

        /// <summary>
        /// 资源包列表弹窗
        /// </summary>
        private class BundleListWindow : EditorWindow
        {
            private RuntimeInfoWindow parent;
            private HashSet<BundleRuntimeInfo> bundleRuntimeInfos;

            public static void OpenWindow(RuntimeInfoWindow parent, HashSet<BundleRuntimeInfo> bundleRuntimeInfos)
            {
                BundleListWindow window = CreateWindow<BundleListWindow>(nameof(BundleListWindow));
                window.ShowPopup();
                window.bundleRuntimeInfos = bundleRuntimeInfos;
                window.parent = parent;
            }

            private void OnGUI()
            {
                if (!Application.isPlaying)
                {
                    Close();
                    return;
                }

                foreach (BundleRuntimeInfo bundleRuntimeInfo in bundleRuntimeInfos)
                {
                    parent.DrawBundleRuntimeInfo(bundleRuntimeInfo);
                }
            }
        }
    }
}

