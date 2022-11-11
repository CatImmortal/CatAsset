using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 依赖关系图窗口
    /// </summary>
    public class DependencyGraphViewWindow: EditorWindow
    {
        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void Open(AssetRuntimeInfo assetRuntimeInfo)
        {
           var window = CreateWindow<DependencyGraphViewWindow>("依赖关系图");
            
            var graphView = new AssetDependencyGraphView()
            {
                style = { flexGrow = 1}
            };

            window.rootVisualElement.Add(graphView);
            window.ShowPopup();
            
            graphView.Init(assetRuntimeInfo);
        }
        
        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                Close();
            }
        }
    }
}