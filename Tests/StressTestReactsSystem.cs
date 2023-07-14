using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Test, Doc.HECS, "this system test components react functionality")]
    public sealed class StressTestReactsSystem : BaseSystem, IReactEntity, IAfterEntityInit,
        IReactCommand<StressTestLocalCommand>, 
        IReactGlobalCommand<StressTestGlobalCommand>,
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
        public bool GlobalReactRemoved = true;
        public bool LocalReact;
        public bool LocalReactRemoved = true;

        public Guid ListenerGuid => SystemGuid;

        public void AfterEntityInit()
        {
            Owner.GetOrAddComponent<TestInitComponent>();
        }

        public void CommandGlobalReact(StressTestGlobalCommand command)
        {
            GlobalReact = command.Param;
            GlobalReactRemoved = command.Param;
        }

        public void CommandReact(StressTestLocalCommand command)
        {
            LocalReact = command.Param;
            LocalReactRemoved = command.Param;
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
            {
                var filter = Owner.World.GetFilter<TestReactComponent>();
                filter.ForceUpdateFilter();

                if (filter.Count > 0)
                    HECSDebug.LogError("in remove component we have problems");

                ReactGlobalRemove = true;
            }    
                
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
        public bool Param;
    }

    public struct StressTestLocalCommand : ICommand
    {
        public bool Param;
    }
}