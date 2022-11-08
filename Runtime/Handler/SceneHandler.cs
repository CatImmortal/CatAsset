using System;
using System.Runtime.CompilerServices;
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
        /// 场景对象
        /// </summary>
        public Scene Scene { get; private set; }

        /// <summary>
        /// 场景加载完毕回调
        /// </summary>
        private SceneLoadedCallback onLoadedCallback;

        /// <summary>
        /// 设置场景对象
        /// </summary>
        internal void SetScene(Scene loadedScene)
        {
            Scene = loadedScene;

            State = loadedScene != default ? HandlerState.Success : HandlerState.Failed;
            
            onLoadedCallback?.Invoke(this);
            ContinuationCallBack?.Invoke();

            if (State == HandlerState.Failed)
            {
                //加载失败 自行释放
                Release();
            }
        }

        /// <inheritdoc />
        public override void Unload()
        {
            if (State == HandlerState.InValid)
            {
                Debug.LogError($"卸载了无效的{GetType().Name}");
                return;
            }
            
            CatAssetManager.UnloadScene(Scene);
            Release();
        }
        
        /// <summary>
        /// 获取可等待对象
        /// </summary>
        public HandlerAwaiter<SceneHandler> GetAwaiter()
        {
            return new HandlerAwaiter<SceneHandler>(this);
        }

        public static SceneHandler Create(string name, SceneLoadedCallback callback)
        {
            SceneHandler handler = ReferencePool.Get<SceneHandler>();
            handler.Name = name;
            handler.State = HandlerState.Doing;
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
