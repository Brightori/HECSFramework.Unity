using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HECSPool<T> : IDisposable where T: UnityEngine.Object 
{
    private Queue<T> stack;
    private Task<T> getNewPooledObject;
    private int maxCount;

    public HECSPool(Task<T> getObject, int maxCount = 50)
    {
        stack = new Queue<T>(maxCount);
        this.maxCount = maxCount;
        getNewPooledObject = getObject;
    }

    public void Dispose()
    {
        getNewPooledObject = null;
        stack.Clear();
    }

    public async Task<T> Get()
    {
        if (stack.Count == 0)
        {
            var refObj = await getNewPooledObject;
            return MonoBehaviour.Instantiate(refObj);
        }
        


        return stack.Dequeue();
    }

    public void Release(T pooledObj)
    {
        if (stack.Count> maxCount)
        {
            //todo надо прописать внятный механизм того что мы делаем если сущностей больше чем надо
            if (pooledObj is IDisposable disposable)
                disposable.Dispose();
            return;
        }

        stack.Enqueue(pooledObj);
    }
}
