using System.Collections.Generic;
using HECSFramework.Core;
using UnityEngine;

namespace Helpers
{
    [Documentation(Doc.HECS, Doc.Poolable, "this is additional helper for getting go with component with sync way")]
    public class HECSSyncPool<T> where T : Component
    {
        private GameObject prfb;
        private Stack<T> pool = new Stack<T>(4);
        public HECSList<T> Items = new HECSList<T>();
        private Queue<T> releaseQueue = new Queue<T>();
        
        public HECSSyncPool(GameObject prfb)
        {
            this.prfb = prfb;
        }

        public T Get(Transform parent = null)
        {
            if (pool.TryPop(out var result))
            {
                if (parent)
                    result.transform.SetParent(parent);

                Items.Add(result);
                result.gameObject.SetActive(true);
                return result;
            }

            var needed = Object.Instantiate(prfb, parent).GetComponent<T>();

            Items.Add(needed);
            return needed;
        }

        public T Get(Vector3 pos, Quaternion quaternion, Transform parent = null)
        {
            if (pool.TryPop(out var result))
            {
                if (parent)
                    result.transform.SetParent(parent);

                Items.Add(result);
                result.transform.SetPositionAndRotation(pos, quaternion);
                result.gameObject.SetActive(true);
                return result;
            }

            var needed = Object.Instantiate(prfb, parent).GetComponent<T>();

            Items.Add(needed);
            return needed;
        }

        public void Release(T value, bool unparent = false)
        {
            if (unparent)
                value.transform.SetParent(null);

            Items.RemoveSwap(value);
            value.gameObject.SetActive(false);
            pool.Push(value);
        }


        /// <summary>
        /// u should use ProcessReleaseQueue after this method, this method just add items what we want to release to queue 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unparent"></param>
        public void ReleaseWithQueue(T value, bool unparent = false)
        {
            if (unparent)
                value.transform.SetParent(null);
            
            value.gameObject.SetActive(false);

            releaseQueue.Enqueue(value);
        }

        public void ProcessReleaseQueue()
        {
            while (releaseQueue.TryDequeue(out var value))
            {
                Items.RemoveSwap(value);
                pool.Push(value);
            }
        }

        public void ReleaseAll(bool unparent = false)
        {
            foreach (var item in Items)
            {
                if (item != null)
                {
                    if (unparent)
                        item.transform.SetParent(null);

                    item.gameObject.SetActive(false);
                    pool.Push(item);
                }
            }

            Items.Clear();
        }
    }
}
