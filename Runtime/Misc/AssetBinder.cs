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
                handler.Unload();
            }
            Handlers.Clear();

        }
    }
}