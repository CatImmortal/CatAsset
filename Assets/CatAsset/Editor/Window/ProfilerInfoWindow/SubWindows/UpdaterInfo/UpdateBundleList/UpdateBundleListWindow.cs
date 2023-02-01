using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using UnityEditor;

namespace CatAsset.Editor
{
    public class UpdateBundleListWindow : EditorWindow
    {
        private UpdateBundleListSubWindow subWindow = new UpdateBundleListSubWindow();
        private List<ProfilerUpdateBundleInfo> infoList;
        public static void Open(List<ProfilerUpdateBundleInfo> infoList)
        {
            var window = CreateWindow<UpdateBundleListWindow>();
            window.infoList = infoList;
            window.Show();
            window.subWindow.TreeView.Reload(infoList);
        }

        private void OnEnable()
        {
            subWindow.InitSubWindow();
        }

        private void OnGUI()
        {
            subWindow.DrawSubWindow(position);
        }
    }
}