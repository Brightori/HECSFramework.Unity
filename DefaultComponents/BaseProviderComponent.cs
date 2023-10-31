using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Provider, "its base component for providing monobehaviours to ecs")]
    public abstract class BaseProviderComponent<T> : BaseComponent, IDisposable, IHaveActor, IAfterEntityInit, IInitAfterView where T : UnityEngine.Component
    {
        [NonSerialized] 
        public T Get;

        public Actor Actor { get; set; }

        public void AfterEntityInit()
        {
            if (Owner.ContainsMask<ViewReferenceGameObjectComponent>()
                && !Owner.ContainsMask<ViewReadyTagComponent>())
                return;

            SetGet();
        }

        public void Dispose()
        {
            Get = null;
            Actor = null;
        }

        public void InitAfterView()
        {
            SetGet();
        }

        public void Reset()
        {
            Get = default;
        }

        private void SetGet()
        {
            Actor.TryGetComponent(out Get, true);
        }
    }
}