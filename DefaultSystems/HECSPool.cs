using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HECSPool<T> : IDisposable where T : UnityEngine.Object
{
    private Queue<T> queue;
    private Task<T> getNewPooledObject;
    private int maxCount;

    public HECSPool(Task<T> getObject, int maxCount = 256)
    {
        queue = new Queue<T>(maxCount);
        this.maxCount = maxCount;
        getNewPooledObject = getObject;
    }

    public void Dispose()
    {
        getNewPooledObject = null;
        queue.Clear();
    }

    public async Task<T> Get()
    {
        if (queue.Count == 0)
        {
            var refObj = await getNewPooledObject;
            return MonoBehaviour.Instantiate(refObj);
        }

        return queue.Dequeue();
    }

    public void Release(T pooledObj)
    {
        if (queue.Count > maxCount)
        {
            //todo надо прописать внятный механизм того что мы делаем если сущностей больше чем надо
            if (pooledObj is IDisposable disposable)
                disposable.Dispose();

            MonoBehaviour.Destroy(pooledObj);
            return;
        }

        queue.Enqueue(pooledObj);
    }
}
