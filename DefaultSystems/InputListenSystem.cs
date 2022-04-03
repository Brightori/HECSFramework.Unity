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
    public class InputListenSystem : BaseSystem, IUpdatable
    {
        private List<UpdateableAction> actions = new List<UpdateableAction>();
        private ConcurrencyList<IEntity> inputListeners;
        private HECSMask InputListenerTagComponentMask = HMasks.GetMask<InputListenerTagComponent>();

        public override void InitSystem()
        {
            inputListeners = EntityManager.Filter(InputListenerTagComponentMask);
            LinkActions();
        }

        private void LinkActions()
        {
            var actionsComponent = Owner.GetHECSComponent<InputActionsComponent>();
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

            IEntity[] array = inputListeners.Data;
            var lenght = inputListeners.Count;
            for (int i = 0; i < lenght; i++)
            {
                IEntity listener = array[i];
                listener.Command(command);
            }
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