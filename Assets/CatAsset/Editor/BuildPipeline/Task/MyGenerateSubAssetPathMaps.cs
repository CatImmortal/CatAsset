using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.WriteTypes;

namespace CatAsset.Editor
{
    /// <summary>
    /// Creates sub asset load information.
    /// </summary>
    public class MyGenerateSubAssetPathMaps : IBuildTask
    {
        /// <inheritdoc />
        public int Version { get { return 1; } }

#pragma warning disable 649
        [InjectContext]
        IBundleWriteData m_WriteData;

        [InjectContext(ContextUsage.In, true)]
        IBuildExtendedAssetData m_ExtendedAssetData;
#pragma warning restore 649

        /// <inheritdoc />
        public ReturnCode Run()
        {
            if (m_ExtendedAssetData == null || m_ExtendedAssetData.ExtendedData == null || m_ExtendedAssetData.ExtendedData.Count == 0)
                return ReturnCode.SuccessNotRun;

            Dictionary<string, IWriteOperation> fileToOperation = m_WriteData.WriteOperations.ToDictionary(x => x.Command.internalName, x => x);

            foreach (var pair in m_ExtendedAssetData.ExtendedData)
            {
                GUID asset = pair.Key;
                string mainFile = m_WriteData.AssetToFiles[asset][0];
                var abOp = fileToOperation[mainFile] as AssetBundleWriteOperation;
            
                int assetInfoIndex = abOp.Info.bundleAssets.FindIndex(x => x.asset == asset);
                AssetLoadInfo assetInfo = abOp.Info.bundleAssets[assetInfoIndex];
                int offset = 1;
                if (pair.Value.Representations.Count > 1000)
                {
                    continue;
                }
                foreach (var subAsset in pair.Value.Representations)
                {
                    var secondaryAssetInfo = CreateSubAssetLoadInfo(assetInfo, subAsset);
                    abOp.Info.bundleAssets.Insert(assetInfoIndex + offset, secondaryAssetInfo);
                    offset++;
                }
            }

            return ReturnCode.Success;
        }

        static AssetLoadInfo CreateSubAssetLoadInfo(AssetLoadInfo assetInfo, ObjectIdentifier subAsset)
        {
            var subAssetLoadInfo = new AssetLoadInfo();
            subAssetLoadInfo.asset = assetInfo.asset;
            subAssetLoadInfo.address = assetInfo.address;
            subAssetLoadInfo.referencedObjects = new List<ObjectIdentifier>(assetInfo.referencedObjects);
            subAssetLoadInfo.includedObjects = new List<ObjectIdentifier>(assetInfo.includedObjects);

            var index = subAssetLoadInfo.includedObjects.IndexOf(subAsset);
            Swap(subAssetLoadInfo.includedObjects,0,index);
            return subAssetLoadInfo;
        }
        
        public static void Swap<T>(IList<T> list, int first, int second)
        {
            T temp = list[second];
            list[second] = list[first];
            list[first] = temp;
        }
    }
}
