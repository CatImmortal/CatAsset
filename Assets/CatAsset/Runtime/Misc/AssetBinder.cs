using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源绑定器
    /// </summary>
    public class AssetBinder : MonoBehaviour
    {
        public readonly List<IBindableHandler> Handlers = new List<IBindableHandler>();

        public void BindTo(IBindableHandler handler)
        {
            Handlers.Add(handler);
        }
        

        private void OnDestroy()
        {
            foreach (IBindableHandler handler in Handlers)
            {
                //使用dispose 在handler的各种状态下都能正确处理
                handler.Dispose();
            }
            Handlers.Clear();

        }
    }
}