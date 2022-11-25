using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace CatAsset.Editor
{
    /// <summary>
    /// 修复SBP打包SpriteAtlas的散图时，纹理冗余的Bug
    /// </summary>
    public class FixSpriteAtlasBug : IBuildTask
    {
        /// <inheritdoc />
        public int Version => 1;
        
        [InjectContext]
        IBundleWriteData writeDataParam;
        
        /// <inheritdoc />
        public ReturnCode Run()
        {
            BundleWriteData writeData = (BundleWriteData)writeDataParam;

            //所有图集散图的guid列集合
            HashSet<GUID> spriteGuids = new  HashSet<GUID>();

            //遍历资源包里的资源 记录其中图集的散图guid
            foreach (var pair in writeData.FileToObjects)
            {
                foreach (ObjectIdentifier objectIdentifier in pair.Value)
                {
                    string path = AssetDatabase.GUIDToAssetPath(objectIdentifier.guid);
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (asset is SpriteAtlas)
                    {
                        List<string> spritePaths = EditorUtil.GetDependencies(path, false);
                        foreach (string spritePath in spritePaths)
                        {
                            GUID spriteGuild = AssetDatabase.GUIDFromAssetPath(spritePath);
                            spriteGuids.Add(spriteGuild);
                        }
                    }
                }
            }

            //将writeData.FileToObjects包含的图集散图的texture删掉 避免冗余
            foreach (var pair in writeData.FileToObjects)
            {
                List<ObjectIdentifier> objectIdentifiers = pair.Value;
                for (int i = objectIdentifiers.Count - 1; i >= 0; i--)
                {
                    ObjectIdentifier objectIdentifier = objectIdentifiers[i];
                    if (spriteGuids.Contains(objectIdentifier.guid))
                    {
                        if (objectIdentifier.localIdentifierInFile == 2800000)
                        {
                            //删除图集散图的冗余texture
                            objectIdentifiers.RemoveAt(i);
                        }
                    }
                }
            }
            
            return ReturnCode.Success;
        }
    }
}