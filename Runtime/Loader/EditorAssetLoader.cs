#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 编辑器资源加载器
    /// </summary>
    public class EditorAssetLoader : BaseAssetLoader
    {
        /// <inheritdoc />
        public override void CheckVersion(OnVersionChecked onVersionChecked)
        {
            VersionCheckResult result = new VersionCheckResult(true, null, 0, 0);
            onVersionChecked?.Invoke(result);
        }
        
        /// <inheritdoc />
        protected override AssetHandler<T> InternalLoadAssetAsync<T>(string assetName, CancellationToken token,
            TaskPriority priority)
        {
            AssetHandler<T> handler;

            if (string.IsNullOrEmpty(assetName))
            {
                handler = AssetHandler<T>.Create();
                handler.Error = "资源名为空";
                handler.SetAsset(null);
                return handler;
            }

            Type assetType = typeof(T);

            AssetCategory category = RuntimeUtil.GetAssetCategoryInEditorMode(assetName, assetType);
            handler = AssetHandler<T>.Create(assetName,token, category);

            object asset;

            if (category == AssetCategory.InternalBundledAsset)
            {
                //加载资源包资源
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, assetType);
            }
            else
            {
                //加载原生资源
                if (category == AssetCategory.ExternalRawAsset)
                {
                    assetName = RuntimeUtil.GetReadWritePath(assetName);
                }

                asset = File.ReadAllBytes(assetName);
            }

            if (asset == null)
            {
                handler.Error = "资源加载失败";
            }

            handler.SetAsset(asset);
            return handler;
        }


        /// <inheritdoc />
        internal override void InternalLoadSceneAsync(string sceneName, SceneHandler handler, TaskPriority priority = TaskPriority.Low)
        {
            LoadSceneParameters param = new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive
            };

            var op = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(sceneName, param);
            if (op == null)
            {
                handler.Error = "场景加载失败";
                handler.SetScene(default);
                return;
            }
                
            op.completed += operation =>
            {
                Scene scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                handler.SetScene(scene);
            };
        }

        /// <inheritdoc />
        public override void UnloadAsset(object asset)
        {
            //编辑器资源模式下不处理Asset的卸载
        }

        /// <inheritdoc />
        public override void UnloadScene(Scene scene)
        {
            if (scene == default || !scene.IsValid() || !scene.isLoaded)
            {
                return;
            }
            SceneManager.UnloadSceneAsync(scene);
        }
    }
}
#endif

