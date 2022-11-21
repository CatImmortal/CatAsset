using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源依赖链节点
    /// </summary>
    public class AssetNode : BaseDependencyNode<AssetRuntimeInfo>
    {
        private readonly ObjectField objFiled;

        private readonly VisualElement infoContainer;

        public override AssetRuntimeInfo Owner
        {
            set
            {
                base.Owner = value;
                objFiled.value = AssetDatabase.LoadAssetAtPath<Object>(Owner.AssetManifest.Name);
                title = Owner.AssetManifest.Name;
                
                //类型名
                var typeName = objFiled.value.GetType().Name;
                var typeLabel = new Label
                {
                    text = $"Type: {typeName}"
                };
                infoContainer.Add(typeLabel);


                //资源预览图
                Texture assetTexture = AssetPreview.GetAssetPreview(objFiled.value);
                if (!assetTexture)
                {
                    assetTexture = AssetPreview.GetMiniThumbnail(objFiled.value);
                }
                if (assetTexture)
                {
                    extensionContainer.Add(new Image
                    {
                        image = assetTexture,
                        scaleMode = ScaleMode.ScaleToFit,
                        style =
                        {
                            width = 100,
                            height = 100,
                        }
                    });
                }
            }
        }

        public AssetNode()
        {
            objFiled = new ObjectField();
            extensionContainer.Add(objFiled);

            infoContainer = new VisualElement
            {
                style =
                {
                    paddingBottom = 4.0f,
                    paddingTop = 4.0f,
                    paddingLeft = 4.0f,
                    paddingRight = 4.0f
                }
            };
            extensionContainer.Add(infoContainer);

            RefreshExpandedState();
        }



    }
}
