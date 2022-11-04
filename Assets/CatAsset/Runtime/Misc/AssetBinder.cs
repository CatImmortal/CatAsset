using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源绑定器
    /// </summary>
    public class AssetBinder : MonoBehaviour
    {
        private readonly List<IBindableHandler> handlers = new List<IBindableHandler>();

        public void BindTo(IBindableHandler handler)
        {
            handlers.Add(handler);
        }


        private void OnDestroy()
        {
            foreach (IBindableHandler handler in handlers)
            {
                handler.Unload();
            }
            handlers.Clear();
            
        }
    }
}