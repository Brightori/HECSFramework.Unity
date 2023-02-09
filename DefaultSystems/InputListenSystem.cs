using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace Systems
{
    [Documentation(Doc.Input, "Система прослушивает юнити инпут и передаёт данные в хекс системы и компоненты.")]
    [Serializable, BluePrint]
    [RequiredAtContainer(typeof(InputActionsComponent))]
    public class InputListenSystem : BaseSystem, IPriorityUpdatable
    {
        private List<UpdateableAction> actions = new List<UpdateableAction>();

        public int Priority { get; } = -5;

        public override void InitSystem()
        {
            LinkActions();
        }

        private void LinkActions()
        {
            var actionsComponent = Owner.GetComponent<InputActionsComponent>();
            var defaultActionMap = actionsComponent.Actions.actionMaps[0];
            defaultActionMap.Enable();

            foreach (var action in defaultActionMap.actions)
            {
                var neededIndex = actionsComponent.InputActionSettings.FirstOrDefault(x => x.ActionName == action.name);
                action.Enable();
                var updateableAction = new UpdateableAction(neededIndex.Identifier.Id, action);
                updateableAction.OnStart += OnActionStart;
                updateableAction.OnEnd += OnActionEnd;
                updateableAction.OnUpdate += OnActionUpdate;
                actions.Add(updateableAction);
            }
        }

        private void OnActionStart(int index, InputAction.CallbackContext context)
        {
            var command = new InputStartedCommand { Index = index, Context = context };
            SendCommandToAllListeners(command);
        }

        private void OnActionUpdate(int index, InputAction.CallbackContext context)
        {
            var command = new InputCommand { Index = index, Context = context };
            SendCommandToAllListeners(command);
        }

        public void OnActionEnd(int index, InputAction.CallbackContext context)
        {
            var command = new InputEndedCommand { Index = index,  Context = context };
            SendCommandToAllListeners(command);
        }

        private void SendCommandToAllListeners<T>(T command) where T : struct, IGlobalCommand
        {
            if (!EntityManager.IsAlive) return;

            foreach (var w in EntityManager.Worlds)
            {
                if (w == null)
                    continue;

                var collection = w.GetFilter<InputListenerTagComponent>();

                foreach (var listener in collection)
                {
                    listener.Command(command);
                }
            }
        }

        public override void Dispose()
        {
            foreach (var action in actions)
                action.Dispose();
        }

        public void PriorityUpdateLocal()
        {
            foreach (var action in actions)
                action.UpdateAction();
        }
    }
}