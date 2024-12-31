using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Systems;
using UnityEngine;

public class HECSPool : IDisposable 
{
    private Queue<GameObject> queue;
    private HashSet<int> alrdyInpool = new HashSet<int>(32);
    private AssetContainer<GameObject> container;
    private readonly Dictionary<int, HECSPool> objectIDToPool;
    private int maxCount;

    public HECSPool(AssetContainer<GameObject> getObject, Dictionary<int, HECSPool> objectIDToPool, int maxCount = 256)
    {
        queue = new Queue<GameObject>(maxCount);
        this.maxCount = maxCount;
        container = getObject;
        this.objectIDToPool = objectIDToPool;
    }

    public void SetMaxCount(int maxCount)
    {
        this.maxCount = maxCount;
    }

    public void Dispose()
    {
        container = default;

        foreach (GameObject obj in queue)
        {
            objectIDToPool.Remove(obj.GetInstanceID());

            if (obj != null)
                MonoBehaviour.Destroy(obj);
        }

        queue.Clear();
    }

    public async UniTask<GameObject> Get(Vector3 position, Quaternion rotation, Transform transform, CancellationToken cancellationToken = default)
    {
    again:

        if (queue.Count == 0)
        {
            var task =  MonoBehaviour.Instantiate<GameObject>(container.CurrentObject, position, rotation, transform);

            if (cancellationToken.IsCancellationRequested)
            {
                MonoBehaviour.Destroy(task);
                throw new OperationCanceledException("[HECSPool] we cancel Get");
            }

            container.RegisterObject(task);
            this.objectIDToPool[task.GetInstanceID()] = this;
            return task;
        }

        var needed = queue.Dequeue();

        if (needed == null)
            goto again;

        alrdyInpool.Remove(needed.GetInstanceID());
        needed.transform.SetPositionAndRotation(position, rotation);
        return needed;
    }

    public void Release(GameObject pooledObj)
    {
        if (pooledObj == null)
            return;

        if (queue.Count > maxCount)
        {
            container.ReleaseObject(pooledObj);
            return;
        }

        if (alrdyInpool.Contains(pooledObj.GetInstanceID()))
            return;

        //SceneManager.MoveGameObjectToScene(pooledObj, SceneManager.GetSceneByBuildIndex(0));
        alrdyInpool.Add(pooledObj.GetInstanceID());
        queue.Enqueue(pooledObj);
    }
}
