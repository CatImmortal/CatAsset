using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    public partial class PackageWindow
    {
        /// <summary>
        /// 需要删除的Rule
        /// </summary>
        private List<int> needRemoveRule = new List<int>();

        /// <summary>
        /// 绘制打包规则界面
        /// </summary>
        private void DrawPackageRuleView()
        {

            EditorGUILayout.Space();

            PackageRuleConfig cfg = Util.PkgRuleCfg;
            bool isNeedSort = false;
            for (int i = 0; i < cfg.Rules.Count; i++)
            {
                PackageRule rule = cfg.Rules[i];

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label($"[{i}]", GUILayout.Width(20));
                    rule.Directory = EditorGUILayout.TextField(rule.Directory);
                    if (GUILayout.Button("选择目录", GUILayout.Width(100)))
                    {
                        string folder = EditorUtility.OpenFolderPanel("选择目录", rule.Directory, "");
                        if (folder != string.Empty)
                        {
                            int AssetsIndex = folder.IndexOf("Assets");
                            folder = folder.Substring(AssetsIndex);
                            rule.Directory = folder;
                            isNeedSort = true;  //需要重新按目录排序
                        }
                    }
                    rule.Mode = (PackageMode)EditorGUILayout.EnumPopup(rule.Mode, GUILayout.Width(100));
                    if (GUILayout.Button("X", GUILayout.Width(50)))
                    {
                        needRemoveRule.Add(i);
                    }
                }
            }

            EditorGUILayout.Space();

            //增加新rule 默认复制最后一个
            if (GUILayout.Button("增加", GUILayout.Width(50)))
            {
                PackageRule rule = new PackageRule();
                if (cfg.Rules.Count > 0)
                {
                    PackageRule lastRule = cfg.Rules[cfg.Rules.Count - 1];
                    rule.Directory = lastRule.Directory;
                    rule.Mode = lastRule.Mode;
                }
                cfg.Rules.Add(rule);
            }

            //删除需要删除的rule
            if (needRemoveRule.Count > 0)
            {
                foreach (int index in needRemoveRule)
                {
                    cfg.Rules.RemoveAt(index);
                }
                needRemoveRule.Clear();
            }

            if (isNeedSort)
            {
                cfg.Rules.Sort();
            }

            EditorUtility.SetDirty(Util.PkgRuleCfg);


        }
    }
}

