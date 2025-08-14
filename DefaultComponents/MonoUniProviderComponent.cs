using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Monobehaviour, Doc.Provider, "this component cache request for mono component and provide fast repeatable access")]
    public sealed class MonoCacheProviderComponent : BaseComponent, IHaveActor, IDisposable
    {
        private List<CacheComponentBase> cached = new List<CacheComponentBase>(2);

        public Actor Actor { get; set; }

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            if (CacheComponent<T>.TryGetComponent(Owner.GUID, out component))
                return true;

            if (Actor.TryGetComponent(out component, true))
            {

                cached.Add(CacheComponent<T>.Add(Owner.GUID, component));
                return true;
            }

            component = null;
            return false;
        }

        public void Dispose()
        {
            foreach (var cache in cached)
            {
                cache.Remove(Owner.GUID);
            }
        }
    }

    public abstract class CacheComponentBase
    {
        public abstract bool Remove(Guid guid);
    }

    public class CacheComponent<T> : CacheComponentBase where T : Component
    {
        private static Dictionary<Guid, T> ActorToComponent = new Dictionary<Guid, T>(2);

        public static CacheComponentBase Add(Guid guid, T element)
        {
            ActorToComponent.Add(guid, element);
            return new CacheComponent<T>();
        }

        public static bool TryGetComponent(Guid guid, out T component)
        {
            return ActorToComponent.TryGetValue(guid, out component);
        }

        public override bool Remove(Guid guid)
        {
            return ActorToComponent.Remove(guid);
        }
    }
}