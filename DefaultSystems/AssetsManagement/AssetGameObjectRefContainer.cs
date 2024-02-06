using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AssetsManagement.Containers
{
    public class AssetGameObjectRefContainer<TRef> : AssetRefContainer<TRef, GameObject>
        where TRef : AssetReference
    {
        public AssetGameObjectRefContainer(TRef loadedReference) : base(loadedReference)
        {
        }
    }
}