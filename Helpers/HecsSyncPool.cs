using System.Collections.Generic;
using HECSFramework.Core;
using UnityEngine;

namespace Helpers
{
    public class HecsSyncPool<T> where T : Component
    {
        private GameObject prfb;
        private Stack<T> pool = new Stack<T>(4);
        public HECSList<T> Items = new HECSList<T>();

        public HecsSyncPool(GameObject prfb)
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
