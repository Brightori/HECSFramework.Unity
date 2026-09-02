using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Helpers;
using Systems;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class HECSPool : IDisposable
{
    private readonly Queue<GameObject> queue;
    private readonly HashSet<int> alreadyInPool = new HashSet<int>(32);
    private readonly AssetService assetService;
    private readonly AssetReference assetReference;
    private readonly Dictionary<int, HECSPool> objectIDToPool;
    private int maxCount;
    private bool isDisposed;

    public AssetReference AssetReference => assetReference;
    public int MaxCount => maxCount;
    public int Count => queue.Count;
    public bool IsDisposed => isDisposed;
    public IEnumerable<GameObject> PooledObjects => queue;

    public GameObject Prefab
        => assetService.TryGetContainer<GameObject>(assetReference, out var container) ? container.CurrentObject : null;

    public HECSPool(AssetService assetService, AssetReference assetReference, Dictionary<int, HECSPool> objectIDToPool, int maxCount = 256)
    {
        queue = new Queue<GameObject>(maxCount);
        this.maxCount = maxCount;
        this.assetService = assetService;
        this.assetReference = assetReference;
        this.objectIDToPool = objectIDToPool;
    }

    public void SetMaxCount(int maxCount)
    {
        this.maxCount = maxCount;
    }

    public async UniTask<GameObject> Get(Vector3 position, Quaternion rotation, Transform parent, CancellationToken cancellationToken = default)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(HECSPool), "[HECSPool] pool of " + assetReference.AssetGUID + " is disposed");

        while (queue.TryDequeue(out var pooled))
        {
            var id = pooled.GetAdaptedInstanceID();
            alreadyInPool.Remove(id);

            if (pooled == null)
            {
                objectIDToPool.Remove(id);
                continue;
            }

            var pooledTransform = pooled.transform;
            pooledTransform.SetPositionAndRotation(position, rotation);
            pooledTransform.SetParent(parent);
            return pooled;
        }

        var instance = await assetService.GetAssetInstance(assetReference, position, rotation, parent, cancellationToken: cancellationToken);
        objectIDToPool[instance.GetAdaptedInstanceID()] = this;
        return instance;
    }

    public void Release(GameObject pooledObj)
    {
        if (pooledObj == null)
            return;

        var id = pooledObj.GetAdaptedInstanceID();

        if (isDisposed || queue.Count >= maxCount)
        {
            objectIDToPool.Remove(id);
            assetService.Release(pooledObj);
            return;
        }

        if (!alreadyInPool.Add(id))
            return;

        objectIDToPool[id] = this;
        queue.Enqueue(pooledObj);
    }

    public void Dispose()
    {
        if (isDisposed)
            return;

        isDisposed = true;

        while (queue.TryDequeue(out var pooled))
        {
            if (pooled == null)
                continue;

            objectIDToPool.Remove(pooled.GetAdaptedInstanceID());
            assetService.Release(pooled);
        }

        alreadyInPool.Clear();
    }
}