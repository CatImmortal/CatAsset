using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源绑定器
    /// </summary>
    public class AssetBinder : MonoBehaviour
    {
        [SerializeField]
        private List<Object> bindingAssets = new List<Object>();

        private List<byte[]> bindingRawAssets = new List<byte[]>();

        /// <summary>
        /// 绑定资源
        /// </summary>
        public void BindTo(object asset)
        {
            if (asset == null)
            {
                return;
            }
            
            if (asset is Object unityObj)
            {
                bindingAssets.Add(unityObj);
            }else if (asset is byte[] rawAsset)
            {
                bindingRawAssets.Add(rawAsset);
            }
        }

        private void OnDestroy()
        {
            foreach (Object asset in bindingAssets)
            {
                CatAssetManager.UnloadAsset(asset);
            }

            foreach (byte[] rawAsset in bindingRawAssets)
            {
                CatAssetManager.UnloadAsset(rawAsset);
            }

        }
    }
}