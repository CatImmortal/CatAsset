using UnityEditor;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 绘制Project窗口下，文件/文件夹描述信息的工具类
    /// </summary>
    public static class DrawDescTool
    {
        private static Color dirColor = Color.gray;
        private static Color assetColor = new Color(0, 0.5f, 0.5f);
        
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            EditorApplication.projectWindowItemOnGUI += OnGUI;
        }

        private static void OnGUI(string guid, Rect selectionRect)
        {

            if (BundleBuildConfigSO.Instance == null)
            {
                return;
            }

            if (BundleBuildConfigSO.Instance.Directories.Count > 0 && BundleBuildConfigSO.Instance.DirectoryDict.Count == 0)
            {
                BundleBuildConfigSO.Instance.RefreshDict();
            }

            
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                //是目录
                if (BundleBuildConfigSO.Instance.DirectoryDict.TryGetValue(path,out BundleBuildDirectory bbd))
                {
                    //绘制资源组在文件夹后面
                    //string desc = $"{bbd.Group}|{bbd.BuildRuleName}";
                    string desc = bbd.Group;
                    DrawDesc(desc,selectionRect,dirColor);
                }

            }
            else
            {
                if (BundleBuildConfigSO.Instance.AssetToBundleDict.TryGetValue(path,out BundleBuildInfo bbi))
                {
                    //绘制资源包相对路径在文件后面
                    string desc = bbi.RelativePath;
                    DrawDesc(desc,selectionRect,assetColor);
                }
            }
        }

        /// <summary>
        /// 绘制Project面板下文件/文件夹后的描述信息
        /// </summary>
        private static void DrawDesc(string desc,Rect selectionRect,Color descColor)
        {

            if (selectionRect.height > 16)
            {
                //图标视图
                return;
            }
            
            GUIStyle label = EditorStyles.label;
            GUIContent content = new GUIContent(desc);
           
         
            Rect pos = selectionRect;

            //只在列表视图绘制
            float width = label.CalcSize(content).x + 10;
            pos.x = pos.xMax - width;  //绘制在最右边
            pos.width = width;
            pos.yMin++;
            
            Color color = GUI.color;
            GUI.color = descColor;
            GUI.DrawTexture(pos, EditorGUIUtility.whiteTexture);
            GUI.color = color;
            GUI.Label(pos, desc);
            
            
        }
    }
}