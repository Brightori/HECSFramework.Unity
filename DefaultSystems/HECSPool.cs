using System;
using System.Collections.Generic;
using AssetsManagement.Containers;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using UnityEngine;

public interface IHECSPool
{
    void SetMaxCount(int maxCount);
    UniTask<GameObject> Get();
    void Release(GameObject pooledObj);
    void Dispose();
}

public class HECSPool<TContainer> : IDisposable, IHECSPool
    where TContainer : IAssetContainer<GameObject>
{
    private Queue<GameObject> queue;
    private TContainer container;
    private int maxCount;

    public HECSPool(TContainer getObject, int maxCount = 256)
    {
        queue = new Queue<GameObject>(maxCount);
        this.maxCount = maxCount;
        container = getObject;
    }

    public void SetMaxCount(int maxCount)
    {
        this.maxCount = maxCount;
    }

    public void Dispose()
    {
        container = default;
        queue.Clear();
    }

    public async UniTask<GameObject> Get()
    {
        if (queue.Count == 0)
        {
            return await container.CreateInstance(Vector3.zero, Quaternion.identity);
        }

        return queue.Dequeue();
    }

    public void Release(GameObject pooledObj)
    {
        if (queue.Count > maxCount)
        {
            container.ReleaseInstance(pooledObj);
            return;
        }

        queue.Enqueue(pooledObj);
    }
}
