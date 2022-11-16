using System.Threading;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 实例化完毕回调方法的原型
    /// </summary>
    public delegate void InstantiateCallback(InstantiateHandler handler);
    
    /// <summary>
    /// 游戏对象实例化句柄
    /// </summary>
    public class InstantiateHandler : BaseHandler
    {
        /// <summary>
        /// 模板
        /// </summary>
        public GameObject Template { get; private set; }
        
        /// <summary>
        /// 父物体
        /// </summary>
        public Transform Parent{ get; private set; }

        /// <summary>
        /// 游戏对象实例
        /// </summary>
        public GameObject Instance{ get; private set; }

        /// <summary>
        /// 实例化完毕回调
        /// </summary>
        private InstantiateCallback onInstantiateCallback;
        
        /// <summary>
        /// 实例化完毕回调
        /// </summary>
        public event InstantiateCallback OnInstantiated
        {
            add
            {
                if (IsDone)
                {
                    value?.Invoke(this);
                    return;
                }

                onInstantiateCallback += value;
            }
            remove
            {
                onInstantiateCallback -= value;
            }
        }

        /// <summary>
        /// 设置游戏对象实例
        /// </summary>
        public void SetInstance(GameObject instance)
        {
            Instance = instance;
            State = Instance != null ? HandlerState.Success : HandlerState.Failed;

            if (CheckTokenCanceled())
            {
                //走到这里 表示是被token取消的 而不是handler.Cancel取消的
                return;
            }

            //未被token取消 检查错误信息 调用回调
            CheckError();
            onInstantiateCallback?.Invoke(this);
            AsyncStateMachineMoveNext?.Invoke();
        }
        
        /// <summary>
        /// 获取可等待对象
        /// </summary>
        public HandlerAwaiter<InstantiateHandler> GetAwaiter()
        {
            if (!IsValid)
            {
                Debug.LogError($"await了一个无效的{GetType().Name}：{Name}");
                return default;
            }
            
            return new HandlerAwaiter<InstantiateHandler>(this);
        }
        
        /// <inheritdoc />
        protected override void InternalUnload()
        {
            Release();
        }

        public static InstantiateHandler Create(string name, CancellationToken token, GameObject template,
            Transform parent)
        {
            InstantiateHandler handler = ReferencePool.Get<InstantiateHandler>();
            handler.CreateBase(name,token);

            handler.Template = template;
            handler.Parent = parent;
            
            return handler;
        }

        public override void Clear()
        {
            base.Clear();
            Template = default;
            Parent = default;
            onInstantiateCallback = default;
        }
    }
}