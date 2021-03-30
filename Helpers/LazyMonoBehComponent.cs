using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity.Helpers
{
    public struct LazyMonoBehComponent<UnityComponent> where UnityComponent : Component
    {
        private UnityComponent unityComponent;
        private bool isInited;
        private IActor actor;

        public LazyMonoBehComponent(IEntity owner) : this()
        {
            if (owner is IActor actor)
                this.actor = actor;
            else
                throw new System.Exception("this entity not actor " + owner.ID + " " + owner.GUID);
        }

        public UnityComponent GetComponent()
        {
            if (isInited)
                return unityComponent;

            if (actor == null)
                return default;

            if (!actor.TryGetComponent(out unityComponent, true))
                Debug.LogAssertion($"нет нужного монобех компонента у {actor.ID} {actor.ID}");
            else
                isInited = true;

            return unityComponent;
        }

        public void SetComponent(UnityComponent unityComponent)
        {
            this.unityComponent = unityComponent;
        }
    }
}