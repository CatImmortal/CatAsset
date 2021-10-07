using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

namespace CatAsset.Editor
{

    public static class OpenDirectory
    {

        [MenuItem("CatAsset/打开目录/资源输出根目录",priority = 2)]
        private static void OpenAssetBundleOutputPath()
        {
            Open(PkgUtil.PkgCfg.OutputPath);
        }

        [MenuItem("CatAsset/打开目录/只读区", priority = 2)]
        private static void OpenReadOnlyPath()
        {
            Open(Application.streamingAssetsPath);
        }

        [MenuItem("CatAsset/打开目录/读写区", priority = 2)]
        private static void OpenReadWritePath()
        {
            Open(Application.persistentDataPath);
        }
        
        private static void Open(string directory)
        {
            directory = string.Format("\"{0}\"", directory);

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                Process.Start("Explorer.exe", directory.Replace('/', '\\'));
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                Process.Start("open", directory);
            }
        }

    }
}

