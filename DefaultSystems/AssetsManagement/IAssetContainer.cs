using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AssetsManagement.Containers
{
    public interface IAssetContainer<TObj>
    {
        public TObj Asset { get; }
    }

    public interface IAssetGameObjectContainer : IAssetContainer<GameObject>
    {
        UniTask<GameObject> CreateInstance(Vector3 pos, Quaternion rot, Transform parent = null);
        UniTask<TComponent> CreateInstanceForComponent<TComponent>(Vector3 pos = default, Quaternion rot = default, Transform parent = null)
            where TComponent : Component;
        void ReleaseInstance(GameObject instance);
    }
}