using UnityEngine;
using UnityEngine.UI;

namespace CatAsset.Runtime
{
    public static class ImageExtensions
    {
        /// <summary>
        /// 设置图片
        /// </summary>
        public static void SetImage(this Image image, string assetName)
        {
            CatAssetManager.LoadAssetAsync<Sprite>(assetName, ((asset, result) =>
            {
                image.sprite = asset;
                image.gameObject.Bind(result.Asset);
            }));
        }
    }
}