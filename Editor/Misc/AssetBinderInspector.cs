﻿using CatAsset.Runtime;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    [CustomEditor(typeof(AssetBinder))]
    public class AssetBinderInspector : UnityEditor.Editor
    {
        private AssetBinder binder;

        private void OnEnable()
        {
            binder = (AssetBinder)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (IBindableHandler bindable in binder.Handlers)
            {
                if (bindable is AssetHandler handler)
                {
                    DrawAssetHandler(handler);
                    
                }else if (bindable is BatchAssetHandler batch)
                {
                    foreach (AssetHandler<object> assetHandler in batch.Handlers)
                    {
                        DrawAssetHandler(assetHandler);
                    }
                }
                
            }
        }

        private void DrawAssetHandler(AssetHandler handler)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                //句柄名
                EditorGUILayout.LabelField(handler.Name);
                    
                //资源对象
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<Object>(handler.Name),typeof(Object),false);
                EditorGUI.EndDisabledGroup();

                //句柄状态
                EditorGUILayout.LabelField(handler.State.ToString());
            }
        }
    }
}