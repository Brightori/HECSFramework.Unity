using System;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Test, Doc.HECS, "this system test components react functionality")]
    public sealed class StressTestReactsSystem : BaseSystem, IReactGenericLocalComponent<ICounter>, IReactGenericGlobalComponent<ICounter>, IReactComponentGlobal<TestReactComponent>, IReactComponentLocal<TestReactComponent>
    {
        public bool ReactGlobalAdd;
        public bool ReactGlobalRemove;

        public bool ReactComponentLocalAdd;
        public bool ReactComponentLocalRemove;

        public bool GenericGlobalAdd;
        public bool GenericGlobalRemove;

        public bool GenericLocalAdd;
        public bool GenericLocalRemove;

        public Guid ListenerGuid => SystemGuid;

        public void ComponentReact(TestReactComponent component, bool isAdded)
        {
            if (component == null || !component.IsAlive)
                throw new Exception("Component null or not alive");

            if (isAdded)
                ReactGlobalAdd = true;
            else
                ReactGlobalRemove = true;
        }

        public void ComponentReact(ICounter component, bool isAdded)
        {
            if (component == null)
                throw new Exception("Generic component null");

            if (isAdded)
                GenericGlobalAdd = true;
            else
                GenericGlobalRemove = true;
        }

        public void ComponentReactGlobal(TestReactComponent component, bool isAdded)
        {
            if (component == null || !component.IsAlive)
                throw new Exception("Component null or not alive");

            if (isAdded)
                ReactGlobalAdd = true;
            else
                ReactGlobalRemove = true;
        }

        public void ComponentReactLocal(ICounter component, bool isAdded)
        {
            if (component == null)
                throw new Exception("Generic component null");

            if (isAdded)
                GenericLocalAdd = true;
            else
                GenericLocalRemove = true;
        }

        public override void InitSystem()
        {
        }
    }
}

namespace Components
{
    public sealed class TestReactComponent : BaseComponent, ICounter
    {
        public int Id { get; }
    }
}