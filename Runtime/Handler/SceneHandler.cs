using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 场景加载完毕回调方法的原型
    /// </summary>
    public delegate void SceneLoadedCallback(SceneHandler handler);

    /// <summary>
    /// 场景句柄
    /// </summary>
    public class SceneHandler : BaseHandler
    {
        /// <summary>
        /// 场景实例
        /// </summary>
        public Scene Scene { get; private set; }

        /// <summary>
        /// 场景加载完毕回调
        /// </summary>
        private SceneLoadedCallback onLoadedCallback;

        /// <inheritdoc />
        public override bool Success => Scene != default;

        /// <summary>
        /// 设置场景实例
        /// </summary>
        internal void SetScene(Scene loadedScene)
        {
            Scene = loadedScene;
            IsDone = true;
            onLoadedCallback?.Invoke(this);
            ContinuationCallBack?.Invoke();

            if (IsValid && !Success)
            {
                //加载失败 自行释放句柄
                Release();
            }
        }

        /// <inheritdoc />
        public override void Unload()
        {
            if (!IsValid)
            {
                Debug.LogError($"卸载了无效的{GetType().Name}");
                return;
            }
            
            CatAssetManager.UnloadScene(Scene);
            Release();
        }

        public static SceneHandler Create(SceneLoadedCallback callback)
        {
            SceneHandler handler = ReferencePool.Get<SceneHandler>();
            handler.IsValid = true;
            handler.onLoadedCallback = callback;
            return handler;
        }

        public override void Clear()
        {
            base.Clear();

            Scene = default;
        }
    }
}
