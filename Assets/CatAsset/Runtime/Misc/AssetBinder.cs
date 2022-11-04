using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源绑定器
    /// </summary>
    public class AssetBinder : MonoBehaviour
    {
        private readonly List<AssetHandler> handlers = new List<AssetHandler>();

        public void BindTo(AssetHandler handler)
        {
            handlers.Add(handler);
        }


        private void OnDestroy()
        {
            foreach (AssetHandler handler in handlers)
            {
                handler.Unload();
            }
            handlers.Clear();
            
        }
    }
}