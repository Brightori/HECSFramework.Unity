using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AssetsManagement.Containers
{
    public interface IAssetContainer{}
    public interface IAssetContainer<TObj> : IAssetContainer
    {
        public TObj Asset { get; }
        public int RefsCount { get; }
        
        UniTask<GameObject> CreateInstance(Vector3 pos, Quaternion rot, Transform parent = null);
        UniTask<TComponent> CreateInstanceForComponent<TComponent>(Vector3 pos = default, Quaternion rot = default, Transform parent = null)
            where TComponent : Component;
        void ReleaseInstance(GameObject instance);
    }
}