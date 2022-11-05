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
        public Scene Scene;

        /// <summary>
        /// 场景加载完毕回调
        /// </summary>
        private SceneLoadedCallback onLoaded;

        /// <summary>
        /// 场景加载完毕回调
        /// </summary>
        public event SceneLoadedCallback OnLoaded
        {
            add
            {
                if (!IsValid)
                {
                    Debug.LogError($"错误的在无效的{GetType().Name}上添加OnLoaded回调");
                    return;
                }
                
                if (IsDone)
                {
                    value?.Invoke(this);
                    return;
                }

                onLoaded += value;
            }

            remove
            {
                if (!IsValid)
                {
                    Debug.LogError($"错误的在无效的{GetType().Name}上移除OnLoaded回调");
                    return;
                }

                onLoaded -= value;
            }
        }

        /// <inheritdoc />
        public override bool Success => Scene != default;

        /// <summary>
        /// 设置场景实例
        /// </summary>
        internal void SetScene(Scene loadedScene)
        {
            Scene = loadedScene;
            IsDone = true;
            onLoaded?.Invoke(this);
            AwaiterContinuation?.Invoke();
        }
        
        /// <inheritdoc />
        public override void Unload()
        {
            CatAssetManager.UnloadScene(Scene);
            Release();
        }

        public static SceneHandler Create()
        {
            SceneHandler handler = ReferencePool.Get<SceneHandler>();
            return handler;
        }

        public override void Clear()
        {
            base.Clear();

            Scene = default;
        }
    }
}