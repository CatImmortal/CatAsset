
using System;
using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 差异包提取窗口
    /// </summary>
    public class DifferenceBundleWindow : EditorWindow
    {
        private string oldBundlesFolder;
        private string newBundlesFolder;
        private string outputFolder;
        
        [MenuItem("CatAsset/打开差异包提取窗口", priority = 2)]
        private static void OpenWindow()
        {
            var window = GetWindow<DifferenceBundleWindow>(false, "差异包提取窗口");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnGUI()
        {
            
            DrawSelectFolder(ref oldBundlesFolder,"旧版本的资源目录:");
            DrawSelectFolder(ref newBundlesFolder,"新版本的资源目录:");
            DrawSelectFolder(ref outputFolder,"差异包输出目录:");

            if (GUILayout.Button("提取差异包",GUILayout.Width(200)))
            {
                OutputDifferenceBundles();
            }
           
        }

        /// <summary>
        /// 绘制选择目录的界面
        /// </summary>
        private void DrawSelectFolder(ref string targetFolder,string tips)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(tips, GUILayout.Width(150));
                targetFolder = GUILayout.TextField(targetFolder,GUILayout.Width(500));
                if (GUILayout.Button("选择目录", GUILayout.Width(100)))
                {
                    string folder = EditorUtility.OpenFolderPanel($"选择{tips}", targetFolder,string.Empty);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        targetFolder = folder;
                    }
                }
            }
        }
        
        /// <summary>
        /// 提取差异包
        /// </summary>
        private void OutputDifferenceBundles()
        {
            string oldManifestPath = Path.Combine(oldBundlesFolder, CatAssetManifest.ManifestBinaryFileName);
            string newManifestPath = Path.Combine(newBundlesFolder, CatAssetManifest.ManifestBinaryFileName);

            if (!File.Exists(oldManifestPath))
            {
                Debug.LogError($"{oldManifestPath}不存在");
                return;
            }
            
            if (!File.Exists(newManifestPath))
            {
                Debug.LogError($"{newManifestPath}不存在");
                return;
            }
            
            byte[] bytes = File.ReadAllBytes(oldManifestPath);
            CatAssetManifest oldManifest = CatAssetManifest.DeserializeFromBinary(bytes);

            bytes = File.ReadAllBytes(newManifestPath);
            CatAssetManifest newManifest = CatAssetManifest.DeserializeFromBinary(bytes);

            //旧资源包集合
            HashSet<BundleManifestInfo> oldBundleSet = new HashSet<BundleManifestInfo>(oldManifest.Bundles);
            
            //差异资源包
            List<BundleManifestInfo> diffBundles = new List<BundleManifestInfo>();

            foreach (BundleManifestInfo bundleManifestInfo in newManifest.Bundles)
            {
                if (!oldBundleSet.Contains(bundleManifestInfo))
                {
                    diffBundles.Add(bundleManifestInfo);
                    Debug.Log("差异包:"  + bundleManifestInfo);
                }
            }
            
            //将差异包输出到指定目录下
            FileInfo fi;
            string targetPath;
            EditorUtil.CreateEmptyDirectory(outputFolder);
            foreach (BundleManifestInfo diffBundle in diffBundles)
            {
                fi = new FileInfo(Path.Combine(newBundlesFolder, diffBundle.RelativePath));
                targetPath = Path.Combine(outputFolder, diffBundle.RelativePath);

                //冗余资源包没有diffBundle.Directory
                if (!string.IsNullOrEmpty(diffBundle.Directory))
                {
                    string targetDirectory = Path.Combine(outputFolder, diffBundle.Directory);
                    if (!Directory.Exists(targetDirectory))
                    {
                        //输出目录不存在则创建
                        Directory.CreateDirectory(targetDirectory);
                    }
                }

                fi.CopyTo(targetPath);
            }
            
            //复制资源清单
            fi = new FileInfo(Path.Combine(newBundlesFolder, CatAssetManifest.ManifestBinaryFileName));
            targetPath = Path.Combine(outputFolder, CatAssetManifest.ManifestBinaryFileName);
            fi.CopyTo(targetPath);
            
            fi = new FileInfo(Path.Combine(newBundlesFolder, CatAssetManifest.ManifestJsonFileName));
            targetPath = Path.Combine(outputFolder, CatAssetManifest.ManifestJsonFileName);
            fi.CopyTo(targetPath);
            
            Debug.Log($"已将差异包复制到{outputFolder}下");
        }
    }
}