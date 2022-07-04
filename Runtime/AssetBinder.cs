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
        private List<Object> bindAssets = new List<Object>();

        private List<byte[]> bindRawAssets = new List<byte[]>();

        /// <summary>
        /// 绑定Asset
        /// </summary>
        public void BindTo(Object asset)
        {
            bindAssets.Add(asset);
        }

        public void BindTo(byte[] rawAsset)
        {
            bindRawAssets.Add(rawAsset);
        }

        private void OnDestroy()
        {
            foreach (Object asset in bindAssets)
            {
                CatAssetManager.UnloadAsset(asset);
            }

            foreach (byte[] rawAsset in bindRawAssets)
            {
                CatAssetManager.UnloadAsset(rawAsset);
            }

        }
    }
}