using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Systems;
using UnityEngine;

#pragma warning disable CS0612, CS0618

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
#if UNITY_2023_3_OR_NEWER
            objectIDToPool.Remove(obj.GetEntityId().GetHashCode());
#else
            objectIDToPool.Remove(obj.GetInstanceID());
#endif
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
            var task = MonoBehaviour.Instantiate<GameObject>(container.CurrentObject, position, rotation, transform);

            if (cancellationToken.IsCancellationRequested)
            {
                MonoBehaviour.Destroy(task);
                throw new OperationCanceledException("[HECSPool] we cancel Get");
            }

            container.RegisterObject(task);

#if UNITY_2023_3_OR_NEWER
            this.objectIDToPool[task.GetEntityId().GetHashCode()] = this;
#else
            this.objectIDToPool[task.GetInstanceID()] = this;
#endif


            return task;
        }

        var needed = queue.Dequeue();

        if (needed == null)
            goto again;

#if UNITY_2023_3_OR_NEWER
        alrdyInpool.Remove(needed.GetEntityId().GetHashCode());
#else
             alrdyInpool.Remove(needed.GetInstanceID());
#endif


        var neededTransform = needed.transform;

        neededTransform.SetPositionAndRotation(position, rotation);
        neededTransform.SetParent(transform);

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

#if UNITY_2023_3_OR_NEWER
        if (alrdyInpool.Contains(pooledObj.GetEntityId().GetHashCode()))
            return;
#else
        if (alrdyInpool.Contains(pooledObj.GetInstanceID()))
            return;
#endif


#if UNITY_2023_3_OR_NEWER
        alrdyInpool.Add(pooledObj.GetEntityId().GetHashCode());
#else
     alrdyInpool.Add(pooledObj.GetInstanceID());
#endif

        //SceneManager.MoveGameObjectToScene(pooledObj, SceneManager.GetSceneByBuildIndex(0));

        queue.Enqueue(pooledObj);
    }
}
