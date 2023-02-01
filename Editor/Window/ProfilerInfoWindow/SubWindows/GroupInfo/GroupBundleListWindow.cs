using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public class GroupBundleListWindow : EditorWindow
    {
        private List<string> bundles;

        private Vector2 scrollPos;
        
        public static void Open(string title, List<string> bundles)
        {
            var window = CreateWindow<GroupBundleListWindow>();
            window.titleContent.text = title;
            window.bundles = bundles;
            window.Show();
        }

        private void OnGUI()
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scroll.scrollPosition;
                foreach (string bundle in bundles)
                {
                    EditorGUILayout.LabelField(bundle);
                }
            }
           
        }
    }
}