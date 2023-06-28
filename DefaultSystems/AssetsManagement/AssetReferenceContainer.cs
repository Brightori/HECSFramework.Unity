using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace AssetsManagement.Containers
{
    public class AssetReferenceContainer<TRef, TObject> : IAssetContainer<TObject>
        where TRef : AssetReference
        where TObject : Object 
    {
        private readonly TObject asset;
        private readonly TRef reference;
        
        private readonly List<GameObject> instances = new List<GameObject>();

        public TObject Asset => asset;
        public TRef Reference => reference;
        public int RefsCount => instances.Count;

        public AssetReferenceContainer(TRef loadedReference)
        {
            asset = loadedReference.Asset as TObject;
            reference = loadedReference;
        }
        
        public async UniTask<GameObject> CreateInstance(Vector3 pos, Quaternion rot, Transform parent = null)
        {
            GameObject instance = await Addressables.InstantiateAsync(reference, pos, rot, parent);
            instances.Add(instance);
            
            return instance;
        }

        public async UniTask<TComponent> CreateInstanceForComponent<TComponent>(Vector3 pos = default, Quaternion rot = default, Transform parent = null)
            where TComponent : Component
        {
            GameObject instance = await CreateInstance(pos, rot, parent);
            return instance != null 
                ? instance.GetComponent<TComponent>()
                :null;
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instances.Remove(instance))
            {
                Addressables.ReleaseInstance(instance);
            }
        }

    }
}