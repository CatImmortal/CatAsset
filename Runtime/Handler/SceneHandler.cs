using System;
using System.Runtime.CompilerServices;
using System.Threading;
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
        /// 资源加载完毕回调
        /// </summary>
        public event SceneLoadedCallback OnLoaded
        {
            add
            {
                if (State == HandlerState.InValid)
                {
                    Debug.LogError($"在无效的{GetType().Name}：{Name}上添加了OnLoaded回调");
                    return;
                }

                if (State != HandlerState.Doing)
                {
                    value?.Invoke(this);
                    return;
                }

                onLoadedCallback += value;
            }

            remove
            {
                if (State == HandlerState.InValid)
                {
                    Debug.LogError($"在无效的{GetType().Name}：{Name}上移除了OnLoaded回调");
                    return;
                }

                onLoadedCallback -= value;
            }
        }
        
        /// <summary>
        /// 设置场景对象
        /// </summary>
        internal void SetScene(Scene loadedScene)
        {
            Task = null;
            Scene = loadedScene;
            State = loadedScene != default ? HandlerState.Success : HandlerState.Failed;
            
            CheckError();
            
            onLoadedCallback?.Invoke(this);
            ContinuationCallBack?.Invoke();
        }

        /// <inheritdoc />
        protected override void InternalUnload()
        {
            CatAssetManager.UnloadScene(this);
            Release();
        }
        
        /// <summary>
        /// 获取可等待对象
        /// </summary>
        public HandlerAwaiter<SceneHandler> GetAwaiter()
        {
            return new HandlerAwaiter<SceneHandler>(this);
        }

        public static SceneHandler Create(string name,CancellationToken token)
        {
            SceneHandler handler = ReferencePool.Get<SceneHandler>();
            handler.CreateBase(name,token);
            return handler;
        }

        public override void Clear()
        {
            base.Clear();

            Scene = default;
        }
    }
}
