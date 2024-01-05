using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AssetsManagement.Containers
{
    public class AssetGameObjectRefContainer<TRef> : AssetRefContainer<TRef, GameObject>
        where TRef : AssetReference
    {
        private readonly List<GameObject> instances = new List<GameObject>();
        public int RefsCount => instances.Count;

        public AssetGameObjectRefContainer(TRef loadedReference) : base(loadedReference)
        {
            
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