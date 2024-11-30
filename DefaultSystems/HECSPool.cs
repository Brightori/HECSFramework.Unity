using System;
using System.Collections.Generic;
using AssetsManagement.Containers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IHECSPool
{
    void SetMaxCount(int maxCount);
    UniTask<GameObject> Get(Vector3 position, Quaternion rotation);
    void Release(GameObject pooledObj);
    void Dispose();
}

public class HECSPool<TContainer> : IDisposable, IHECSPool
    where TContainer : IAssetContainer<GameObject>
{
    private Queue<GameObject> queue;
    private HashSet<int> alrdyInpool = new HashSet<int>(32);
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

        foreach (GameObject obj in queue)
        {
            if (obj != null)
                MonoBehaviour.Destroy(obj);
        }

        queue.Clear();
    }

    public async UniTask<GameObject> Get(Vector3 position, Quaternion rotation)
    {
    again:

        if (queue.Count == 0)
        {
            return await container.CreateInstance(position, rotation);
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
            container.ReleaseInstance(pooledObj);
            return;
        }

        if (alrdyInpool.Contains(pooledObj.GetInstanceID()))
            return;

        //SceneManager.MoveGameObjectToScene(pooledObj, SceneManager.GetSceneByBuildIndex(0));
        alrdyInpool.Add(pooledObj.GetInstanceID());
        queue.Enqueue(pooledObj);
    }
}
