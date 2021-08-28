using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class PackageWindow
    {
        /// <summary>
        /// 是否初始化过资源预览界面
        /// </summary>
        private bool isInitAssetsPreviewView;

        /// <summary>
        /// 资源预览中的各assetbundle是否已展开
        /// </summary>
        private Dictionary<string, bool> abFoldOut = new Dictionary<string, bool>();

        /// <summary>
        /// 要打包的AssetBundleBuild列表
        /// </summary>
        private List<AssetBundleBuild> abBuildList;

        /// <summary>
        /// 初始化资源预览界面
        /// </summary>
        private void InitAssetsPreviewView()
        {
            abFoldOut.Clear();
            abBuildList = Util.PkgRuleCfg.GetAssetBundleBuildList(isAnalyzeRedundancy);
            foreach (AssetBundleBuild abBuild in abBuildList)
            {
                abFoldOut[abBuild.assetBundleName] = false;
            }
        }

        /// <summary>
        /// 绘制资源预览界面
        /// </summary>
        private void DrawAssetsPreviewView()
        {
            if (!isInitAssetsPreviewView)
            {
                InitAssetsPreviewView();
                isInitAssetsPreviewView = true;
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("全部展开", GUILayout.Width(100)))
                {
                    foreach (AssetBundleBuild abBuild in abBuildList)
                    {
                        abFoldOut[abBuild.assetBundleName] = true;
                    }
                }

                if (GUILayout.Button("全部收起", GUILayout.Width(100)))
                {
                    foreach (AssetBundleBuild abBuild in abBuildList)
                    {
                        abFoldOut[abBuild.assetBundleName] = false;
                    }
                }



                if (GUILayout.Button("刷新", GUILayout.Width(100)))
                {
                    InitAssetsPreviewView();
                }
            }



            foreach (AssetBundleBuild abBuild in abBuildList)
            {
                abFoldOut[abBuild.assetBundleName] = EditorGUILayout.Foldout(abFoldOut[abBuild.assetBundleName], abBuild.assetBundleName);

                if (abFoldOut[abBuild.assetBundleName] == true)
                {
                    foreach (string assetName in abBuild.assetNames)
                    {
                        EditorGUILayout.LabelField("\t" + assetName);
                    }
                }
            }
        }
    }
}

