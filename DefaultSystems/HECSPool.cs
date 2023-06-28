using System;
using System.Collections.Generic;
using AssetsManagement.Containers;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using UnityEngine;

public class HECSPool<TContainer> : IDisposable
    where TContainer : IAssetContainer<GameObject>
{
    private Queue<GameObject> queue;
    private UniTask<TContainer> containerTask;
    private int maxCount;

    public HECSPool(UniTask<TContainer> getObject, int maxCount = 256)
    {
        queue = new Queue<GameObject>(maxCount);
        this.maxCount = maxCount;
        containerTask = getObject;
    }

    public void SetMaxCount(int maxCount)
    {
        this.maxCount = maxCount;
    }

    public void Dispose()
    {
        containerTask = default;
        queue.Clear();
    }

    public async UniTask<GameObject> Get()
    {
        if (queue.Count == 0)
        {
            TContainer container = await containerTask;
            return await container.CreateInstance(Vector3.zero, Quaternion.identity);
        }

        return queue.Dequeue();
    }

    public void Release(GameObject pooledObj)
    {
        var awaiter = containerTask.GetAwaiter();
        if (!awaiter.IsCompleted)
        {
            HECSDebug.LogError("Try to release obj from container that is not ready");
            return;
        }
        if (queue.Count > maxCount)
        {
            awaiter.GetResult().ReleaseInstance(pooledObj);
            return;
        }

        queue.Enqueue(pooledObj);
    }
}
