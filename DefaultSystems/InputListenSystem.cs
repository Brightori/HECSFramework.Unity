using System;
using System.Collections.Generic;
using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Network;
using HECSFramework.Unity;
using UnityEngine.InputSystem;

namespace Systems
{
    [Documentation(Doc.Input, "Система прослушивает юнити инпут и передаёт данные в хекс системы и компоненты.")]
    [Serializable, BluePrint]
    public class InputListenSystem : BaseSystem, INotReplicable, IUpdatable
    {
        private List<UpdateableAction> actions = new List<UpdateableAction>();
        private ConcurrencyList<IEntity> inputListeners;

        public override void InitSystem()
        {
            inputListeners = EntityManager.Filter(HMasks.InputListenerTagComponent);
            LinkActions();
        }

        private void LinkActions()
        {
            var actionsComponent = Owner.GetInputActionsComponent();

            foreach (var action in actionsComponent.Actions)
            {
                action.Enable();
                var updateableAction = new UpdateableAction(action);
                updateableAction.OnStart += OnActionStart;
                updateableAction.OnEnd += OnActionEnd;
                updateableAction.OnUpdate += OnActionUpdate;
                actions.Add(updateableAction);
            }
        }

        private void OnActionStart(InputAction.CallbackContext context)
        {
            var command = new InputStartedCommand {Context = context};
            foreach (var listener in inputListeners) listener.Command(command);
            EntityManager.GlobalCommand(command);
        }

        private void OnActionUpdate(InputAction.CallbackContext context)
        {
             var command = new InputCommand {Context = context};
             foreach (var listener in inputListeners) listener.Command(command);
             EntityManager.GlobalCommand(command);
        }

        public void OnActionEnd(InputAction.CallbackContext context)
        {
            var command = new InputEndedCommand {Context = context};
            foreach (var listener in inputListeners) listener.Command(command);
            EntityManager.GlobalCommand(command);
        }

        public override void Dispose()
        {
            foreach (var action in actions) 
                action.Dispose();
        }

        public void UpdateLocal()
        {
            foreach (var action in actions) 
                action.UpdateAction();
        }
    }
}