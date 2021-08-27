using System.Collections;
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
    public class AssetRuntimeInfoWindow : EditorWindow
    {
        /// <summary>
        /// 选择的页签
        /// </summary>
        private int selectedTab;

        /// <summary>
        /// 页签
        /// </summary>
        private string[] tabs = { "资源信息", "任务信息" };

        private bool isInitAssetInfoView;
        private Dictionary<string, AssetBundleRuntimeInfo> assetBundleInfoDict;
        private Dictionary<string, AssetRuntimeInfo> assetInfoDict;

        /// <summary>
        /// 资源信息中的各assetbundle是否已展开
        /// </summary>
        private Dictionary<string, bool> abFoldOut = new Dictionary<string, bool>();

        private bool isInitTaskInfoView;
        private Dictionary<string, BaseTask> taskDict;

        [MenuItem("CatAsset/打开资源运行时信息窗口", priority = 1)]
        private static void OpenWindow()
        {
            AssetRuntimeInfoWindow window = GetWindow<AssetRuntimeInfoWindow>(false, "资源运行时信息窗口");
            window.minSize = new Vector2(800, 600);
            window.Show();
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
            }

            Repaint();
            
        }

        /// <summary>
        /// 初始化资源信息界面
        /// </summary>
        private void InitAssetInfoView()
        {
            assetBundleInfoDict = typeof(CatAssetManager).GetField("assetBundleInfoDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, AssetBundleRuntimeInfo>;
            assetInfoDict = typeof(CatAssetManager).GetField("assetInfoDict", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<string, AssetRuntimeInfo>;

        }

        /// <summary>
        /// 绘制资源信息界面
        /// </summary>
        private void DrawAssetInfoView()
        {
            if (!isInitAssetInfoView)
            {
                isInitAssetInfoView = true;
                InitAssetInfoView();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("\tAsset名称");
                EditorGUILayout.LabelField("\tAsset实例");
                EditorGUILayout.LabelField("\t引用计数");
            }

            foreach (KeyValuePair<string, AssetBundleRuntimeInfo> item in assetBundleInfoDict)
            {
                string abName = item.Key;
                AssetBundleRuntimeInfo abInfo = item.Value;
                if (abInfo.UsedAsset.Count > 0)
                {
                   

                    //只绘制有Asset在使用中的AssetBundle
                    if (!abFoldOut.ContainsKey(abName))
                    {
                        abFoldOut.Add(abName, false);
                    }

                    abFoldOut[abName] = EditorGUILayout.Foldout(abFoldOut[abName], abName);
                    if (abFoldOut[abName] == true)
                    {
                       

                        foreach (AssetManifestInfo assetManifestInfo in abInfo.ManifestInfo.Assets)
                        {


                            string assetName = assetManifestInfo.AssetName;
                            AssetRuntimeInfo assetInfo = assetInfoDict[assetName];
                            if (assetInfo.UseCount == 0)
                            {
                                continue;
                            }

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField("\t" + assetName);
                                if (assetInfo.Asset)
                                {
                                    EditorGUILayout.LabelField("\t" + assetInfo.Asset.GetType().Name);
                                }
                                else
                                {
                                    EditorGUILayout.LabelField("\tScene");
                                }
                                
                                EditorGUILayout.LabelField("\t" + assetInfo.UseCount.ToString());
                            }
                        }

                        EditorGUILayout.Space();
                    }
                }
            }
        }

        /// <summary>
        /// 初始化任务信息界面
        /// </summary>
        private void InitTaksInfoView()
        {
            TaskExcutor taskExcutor = typeof(CatAssetManager).GetField("taskExcutor", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as TaskExcutor;
            taskDict = typeof(TaskExcutor).GetField("taskDict", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(taskExcutor) as Dictionary<string, BaseTask>;
        }

        /// <summary>
        /// 绘制任务信息界面
        /// </summary>
        private void DrawTaskInfoView()
        {
            if (!isInitTaskInfoView)
            {
                isInitTaskInfoView = true;
                InitTaksInfoView();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("任务名称");
                EditorGUILayout.LabelField("任务类型");
                EditorGUILayout.LabelField("任务状态");
                EditorGUILayout.LabelField("任务进度");
            }

            foreach (KeyValuePair<string, BaseTask> item in taskDict)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    BaseTask task = item.Value;
                    EditorGUILayout.LabelField(task.Name);
                    EditorGUILayout.LabelField(task.GetType().Name);
                    EditorGUILayout.LabelField(task.State.ToString());
                    EditorGUILayout.LabelField(task.Progress.ToString("0.00"));
                }
               
            }
        }
    
    
    }
}

