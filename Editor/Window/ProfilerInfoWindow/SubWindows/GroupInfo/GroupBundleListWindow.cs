using System;
using System.Collections.Generic;
using UnityEditor;

namespace CatAsset.Editor
{
    public class GroupBundleListWindow : EditorWindow
    {
        private List<string> bundles;
        
        public static void Open(string title, List<string> bundles)
        {
            var window = CreateWindow<GroupBundleListWindow>();
            window.titleContent.text = title;
            window.bundles = bundles;
        }

        private void OnGUI()
        {
            foreach (string bundle in bundles)
            {
                EditorGUILayout.LabelField(bundle);
            }
        }
    }
}