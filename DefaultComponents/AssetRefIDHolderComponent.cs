using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Components
{
    [Serializable][Documentation(Doc.Local, Doc.Visual, Doc.View, "here we hold pair of ID and AssetReference to various logic scenarios and for using by actions")]
    public sealed class AssetRefIDHolderComponent : BaseComponent
    {
        [SerializeField] private IDToAssetRef[] iDToAssetRefs = new IDToAssetRef[0]; 

        public AssetReference GetRef (int id)
        {
            for (int i = 0; i < iDToAssetRefs.Length; i++)
            {
                var data = iDToAssetRefs[i];

                if (data.AssetRefID == id)
                    return data.AssetReference;
            }

            return null;
        }

        public bool TryGetRef(int id, out AssetReference assetReference)
        {
            for (int i = 0; i < iDToAssetRefs.Length; i++)
            {
                var data = iDToAssetRefs[i];

                if (data.AssetRefID == id)
                {
                    assetReference = data.AssetReference;
                    return true;
                }
                    
            }

            assetReference = null;
            return false;
        }
    }

    [Serializable]
    public class IDToAssetRef
    {
        public AssetRefID AssetRefID;
        public AssetReference AssetReference;
    }
}