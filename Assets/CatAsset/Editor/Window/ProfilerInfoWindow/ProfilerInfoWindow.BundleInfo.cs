using System;
using System.Collections.Generic;
using System.Reflection;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class ProfilerInfoWindow
    {
        private List<ProfilerBundleInfo> bundleInfoList;

        private Vector2 scrollPos;

        /// <summary>
        /// 是否只显示主动加载的资源
        /// </summary>
        private bool isOnlyShowActiveLoad = false;

        /// <summary>
        /// 资源包相对路径->是否展开
        /// </summary>
        private Dictionary<string, bool> foldOutDict = new Dictionary<string, bool>();

        private void ClearBundleInfoView()
        {
            bundleInfoList = null;
        }

        /// <summary>
        /// 绘制运行时信息界面
        /// </summary>
        private void DrawBundleInfoView()
        {
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

                if (bundleInfoList == null)
                {
                    return;
                }

                foreach (var profilerBundleInfo in bundleInfoList)
                {
                    string relativePath = profilerBundleInfo.RelativePath;

                    if (!foldOutDict.ContainsKey(relativePath))
                    {
                        foldOutDict.Add(relativePath, false);
                    }

                    if (isAllFoldOutTrue)
                    {
                        //点击过全部展开
                        foldOutDict[relativePath] = true;
                    }
                    else if (isAllFoldOutFalse)
                    {
                        //点击过全部收起
                        foldOutDict[relativePath] = false;
                    }

                    if (isOnlyShowActiveLoad)
                    {
                        //仅显示主动加载的资源
                        //此资源包至少有一个主动加载的资源，才能显示
                        bool canShow = false;
                        foreach (var profilerAssetInfo in profilerBundleInfo.InMemoryAssets)
                        {
                            if (profilerAssetInfo.RefCount > 0 && profilerAssetInfo.RefCount != profilerAssetInfo.DependencyChain.DownStream.Count)
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
                        foldOutDict[relativePath] = EditorGUILayout.Foldout(foldOutDict[relativePath], relativePath);

                        DrawProfilerBundleInfo(profilerBundleInfo,false);
                    }

                    if (foldOutDict[relativePath])
                    {
                        EditorGUILayout.Space();

                        foreach (var profilerAssetInfo in profilerBundleInfo.InMemoryAssets)
                        {
                            if (isOnlyShowActiveLoad && profilerAssetInfo.RefCount > 0 && profilerAssetInfo.RefCount == profilerAssetInfo.DependencyChain.DownStream.Count)
                            {
                                //只显示主动加载的资源 且 此资源是纯被依赖加载的 就跳过
                                continue;
                            }

                            DrawProfilerAssetInfo(profilerAssetInfo);
                        }

                        EditorGUILayout.Space();
                    }
                }
            }


        }

        /// <summary>
        /// 绘制分析器资源包信息
        /// </summary>
        private void DrawProfilerBundleInfo(ProfilerBundleInfo profilerBundleInfo,bool isDrawName = true)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (isDrawName)
                {
                    EditorGUILayout.LabelField($"{profilerBundleInfo.RelativePath}" ,GUILayout.Width(400));
                }
                else
                {
                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField($"资源组：{profilerBundleInfo.Group}" ,GUILayout.Width(150));
                EditorGUILayout.LabelField($"内存中资源数：{profilerBundleInfo.InMemoryAssets.Count}/{profilerBundleInfo.TotalAssetCount}",GUILayout.Width(125));
                EditorGUILayout.LabelField($"引用中资源数：{profilerBundleInfo.ReferencingAssetCount}/{profilerBundleInfo.TotalAssetCount}" ,GUILayout.Width(125));
                EditorGUILayout.LabelField($"文件长度：{RuntimeUtil.GetByteLengthDesc(profilerBundleInfo.Length)}",GUILayout.Width(125));

                EditorGUILayout.LabelField($"上游资源包数量：{profilerBundleInfo.DependencyChain.UpStream.Count}",GUILayout.Width(125));
                EditorGUILayout.LabelField($"下游资源包数量：{profilerBundleInfo.DependencyChain.DownStream.Count}",GUILayout.Width(125));


                // if (GUILayout.Button("查看资源包依赖关系图") && (profilerBundleInfo.DependencyChain.UpStream.Count > 0 || profilerBundleInfo.DependencyChain.DownStream.Count > 0))
                // {
                //     DependencyGraphViewWindow.Open<BundleRuntimeInfo,BundleNode>(profilerBundleInfo);
                // }

                EditorGUILayout.LabelField("", GUILayout.Width(30));
            }


        }

        /// <summary>
        /// 绘制分析器资源信息
        /// </summary>
        private void DrawProfilerAssetInfo(ProfilerAssetInfo profilerAssetInfo)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                //资源名
                EditorGUILayout.LabelField("\t" + profilerAssetInfo.Name);

                //对象引用
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(profilerAssetInfo.Name), typeof(UnityEngine.Object),false);
                EditorGUI.EndDisabledGroup();

                //长度
                EditorGUILayout.LabelField($"\t长度：{RuntimeUtil.GetByteLengthDesc(profilerAssetInfo.Length)}");

                //引用计数
                EditorGUILayout.LabelField($"\t引用计数：{profilerAssetInfo.RefCount}");

                //上游资源数
                EditorGUILayout.LabelField($"\t上游资源数量：{profilerAssetInfo.DependencyChain.UpStream.Count}");

                //下游资源数
                EditorGUILayout.LabelField($"\t下游资源数量：{profilerAssetInfo.DependencyChain.DownStream.Count}");

                // if (GUILayout.Button("查看资源依赖关系图") && (profilerAssetInfo.DependencyChain.UpStream.Count > 0 || profilerAssetInfo.DependencyChain.DownStream.Count > 0))
                // {
                //     DependencyGraphViewWindow.Open<AssetRuntimeInfo,AssetNode>(profilerAssetInfo);
                // }
            }
        }
    }
}

