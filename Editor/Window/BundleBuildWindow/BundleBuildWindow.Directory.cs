using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class BundleBuildWindow
    {
        /// <summary>
        /// 构建规则名列表
        /// </summary>
        private string[] ruleNames;

        /// <summary>
        /// 规则名->索引
        /// </summary>
        private Dictionary<string, int> ruleNameDict = new Dictionary<string, int>();

        /// <summary>
        /// 需要删除的目录索引
        /// </summary>
        private readonly List<int> needRemoveDirectories = new List<int>();
        
        /// <summary>
        /// 绘制资源包构建目录界面
        /// </summary>
        private void DrawBundleBuildDirectoryView()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();

            for (int i = 0; i < bundleBuildConfg.Directories.Count; i++)
            {
                BundleBuildDirectory directory = bundleBuildConfg.Directories[i];

                using (new EditorGUILayout.HorizontalScope())
                {
                    //绘制序号
                    GUILayout.Label($"[{i}]", GUILayout.Width(20));
                    
                    //绘制目录名
                    directory.DirectoryName = EditorGUILayout.TextField(directory.DirectoryName);

                    //绘制构建规则名
                    string[] ruleNames = GetRuleNames();
                    ruleNameDict.TryGetValue(directory.BuildRuleName, out int index);
                    index = EditorGUILayout.Popup(index,ruleNames);
                    directory.BuildRuleName = ruleNames[index];
                  
                    
                    //绘制资源组
                    EditorGUILayout.LabelField("资源组：",GUILayout.Width(50));
                    directory.Group = EditorGUILayout.TextField(directory.Group,GUILayout.Width(100));
                    
                    //绘制删除按钮
                    if (GUILayout.Button("X", GUILayout.Width(50)))
                    {
                        needRemoveDirectories.Add(i);
                    }
                }

            }

            //删除需要删除的目录
            if (needRemoveDirectories.Count > 0)
            {
                foreach (int index in needRemoveDirectories)
                {
                    bundleBuildConfg.Directories.RemoveAt(index);
                }
                needRemoveDirectories.Clear();
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(bundleBuildConfg);
                AssetDatabase.SaveAssets();
            }

        }

        /// <summary>
        /// 获取构建规则名列表
        /// </summary>
        /// <returns></returns>
        private string[] GetRuleNames()
        {
            if (ruleNames == null || ruleNameDict.Count == 0)
            {
                List<string> list = new List<string>();
                Type[] types = typeof(BundleBuildConfigSO).Assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (!type.IsInterface && typeof(IBundleBuildRule).IsAssignableFrom(type))
                    {
                        list.Add(type.Name);
                    }
                }
                ruleNames = list.ToArray();
                ruleNameDict.Clear();
                for (int i = 0; i < ruleNames.Length; i++)
                {
                    ruleNameDict.Add(ruleNames[i],i);
                }
                
            }
            
            return ruleNames;
        }
    }
}