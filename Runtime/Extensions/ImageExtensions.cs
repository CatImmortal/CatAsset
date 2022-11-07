﻿using UnityEngine;
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
            AssetHandler<Sprite> handler =  CatAssetManager.LoadAssetAsync<Sprite>(assetName);
           handler.OnLoaded += (assetHandler =>
           {
               image.sprite = assetHandler.Asset;
               image.gameObject.Bind(assetHandler);
           });
        }
    }
}