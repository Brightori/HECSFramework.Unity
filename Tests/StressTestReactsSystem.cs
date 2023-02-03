using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Test, Doc.HECS, "this system test components react functionality")]
    public sealed class StressTestReactsSystem : BaseSystem, IReactEntity, IReactCommand<StressTestLocalCommand>, IReactGlobalCommand<StressTestGlobalCommand>,
        IReactGenericLocalComponent<ICounter>, 
        IReactGenericGlobalComponent<ICounter>, 
        IReactComponentGlobal<TestReactComponent>, 
        IReactComponentLocal<TestReactComponent>
    {
        public bool ReactGlobalAdd;
        public bool ReactGlobalRemove;

        public bool ReactComponentLocalAdd;
        public bool ReactComponentLocalRemove;

        public bool GenericGlobalAdd;
        public bool GenericGlobalRemove;

        public bool GenericLocalAdd;
        public bool GenericLocalRemove;

        public bool EntityAdded;
        public bool EntityRemoved;

        public bool GlobalReact;
        public bool LocalReact;

        public Guid ListenerGuid => SystemGuid;

        public void CommandGlobalReact(StressTestGlobalCommand command)
        {
            GlobalReact = true;
        }

        public void CommandReact(StressTestLocalCommand command)
        {
            LocalReact = true;
        }

        public void ComponentReact(TestReactComponent component, bool isAdded)
        {
            if (component == null)
                throw new Exception("Component null or not alive");

            if (isAdded)
                ReactComponentLocalAdd = true;
            else
                ReactComponentLocalRemove = true;
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
            if (component == null)
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

        public void EntityReact(Entity entity, bool isAdded)
        {
            if (isAdded)
                EntityAdded = true;
            else
                EntityRemoved = true;
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

namespace Commands
{
    public struct StressTestGlobalCommand : IGlobalCommand
    {
    }

    public struct StressTestLocalCommand : ICommand
    {
    }
}