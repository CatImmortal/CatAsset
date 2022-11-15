using CatAsset.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace CatAsset.Editor
{
    public class AssetNode : BaseDependencyNode<AssetRuntimeInfo>
    {
        private readonly ObjectField objFiled;

        public override AssetRuntimeInfo Owner
        {
            set
            {
                base.Owner = value;
                title = Owner.AssetManifest.Name;
                objFiled.value = AssetDatabase.LoadAssetAtPath<Object>(Owner.AssetManifest.Name);
            }
        }

        public AssetNode()
        {
            objFiled = new ObjectField();
            mainContainer.Add(objFiled);
        }

        

    }
}
