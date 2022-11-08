using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源绑定器
    /// </summary>
    public class AssetBinder : MonoBehaviour
    {
        public readonly List<AssetHandler> Handlers = new List<AssetHandler>();

        public void BindTo(AssetHandler handler)
        {
            Handlers.Add(handler);
        }


        private void OnDestroy()
        {
            foreach (AssetHandler handler in Handlers)
            {
                //使用dispose 方便在handler的各种情况下都能正确处理
                handler.Dispose();
            }
            Handlers.Clear();
            
        }
    }
}