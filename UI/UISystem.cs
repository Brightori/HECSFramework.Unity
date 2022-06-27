using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Systems
{
    [Serializable, BluePrint]
    [Documentation(Doc.UI, Doc.HECS, "This system default for operating ui at hecs, this system have command for show and hide ui plus show or hide ui groups, this system still in progress")]
    public class UISystem : BaseSystem, IUISystem
    {
        public const string UIBluePrints = "UIBluePrints";

        private Queue<IGlobalCommand> commandsQueue = new Queue<IGlobalCommand>();

        private ConcurrencyList<IEntity> uiCurrents;

        private UnityTransformComponent mainCanvasTransform;
        private List<UIBluePrint> uIBluePrints = new List<UIBluePrint>();

        private HECSMask uiTagMask = HMasks.GetMask<UITagComponent>();
        private HECSMask transformComponent = HMasks.GetMask<TransformComponent>();
        private HECSMask uiGroupTagMask = HMasks.GetMask<UIGroupTagComponent>();
        private HECSMask mainCanvasTagComponentMask = HMasks.GetMask<MainCanvasTagComponent>();
        private HECSMask uiTagComponentMask = HMasks.GetMask<UITagComponent>();

        private bool isReady;
        private bool isLoaded;

        public override void InitSystem()
        {
            uiCurrents = EntityManager.Filter(new FilterMask(uiTagMask));
            Addressables.LoadAssetsAsync<UIBluePrint>(UIBluePrints, null).Completed += LoadReact;
        }

        private void LoadReact(AsyncOperationHandle<IList<UIBluePrint>> obj)
        {
            foreach (var bp in obj.Result)
                uIBluePrints.Add(bp);

            isLoaded = true;

            if (isLoaded && isReady)
                ProcessQueue();
        }

        public void CommandGlobalReact(ShowUICommand command)
        {
            if (!isLoaded || !isReady)
            {
                commandsQueue.Enqueue(command);
                return;
            }

            if (!command.MultyView)
            {
                foreach (var ui in uiCurrents)
                {
                    if (ui == null || !ui.IsAlive)
                        continue;

                    var uiTag = ui.GetHECSComponent<UITagComponent>(ref uiTagComponentMask);

                    if (uiTag.ViewType.Id == command.UIViewType)
                    {
                        uiTag.Owner.Command(command);
                        command.OnUILoad?.Invoke(uiTag.Owner);
                        return;
                    }
                }
            }

            var spawn = uIBluePrints.FirstOrDefault(x => x.UIType.Id == command.UIViewType);

            if (spawn == null)
            {
                Debug.LogAssertion("��� ������� ui ��������� " + command.UIViewType);
                return;
            }

            SpawnUIFromBluePrint(spawn, command.OnUILoad);
        }

        private void SpawnUIFromBluePrint(UIBluePrint spawn, Action<IEntity> action)
        {
            Addressables.InstantiateAsync(spawn.UIActor, mainCanvasTransform.Transform).Completed += a => LoadUI(a, action);
        }

        private void LoadUI(AsyncOperationHandle<GameObject> obj, Action<IEntity> onUILoad)
        {
            if (obj.Result.TryGetComponent<UIActor>(out var actor))
            {
                actor.Init();
                actor.Command(new ShowUICommand());
                onUILoad?.Invoke(actor);
            }
            else
                Debug.LogAssertion("this is not UIActor " + obj.Result.name);
        }

        public void CommandGlobalReact(HideUICommand command)
        {
            if (!isLoaded && !isReady)
            {
                commandsQueue.Enqueue(command);
                return;
            }

            foreach (var ui in uiCurrents)
            {
                if (ui == null || !ui.IsAlive)
                    continue;

                var uiTag = ui.GetHECSComponent<UITagComponent>(ref uiTagComponentMask);

                if (uiTag.ViewType.Id == command.UIViewType)
                {
                    uiTag.Owner.Command(command);
                    return;
                }
            }
        }

        public void CommandGlobalReact(CanvasReadyCommand command)
        {
            if (EntityManager.TryGetEntityByComponents(out var canvas, ref mainCanvasTagComponentMask))
            {
                if (!canvas.TryGetHecsComponent(HMasks.UnityTransformComponent, out mainCanvasTransform))
                    Debug.LogAssertion("��� ���������� � ���� �������");
            }
            else
                Debug.LogAssertion("�� ����� ���� �������");

            isReady = true;

            if (isLoaded && isReady)
                ProcessQueue();
        }

        private void ProcessQueue()
        {
            while (commandsQueue.Count > 0)
            {
                var command = commandsQueue.Dequeue();

                switch (command)
                {
                    case ShowUICommand showUI:
                        CommandGlobalReact(showUI);
                        break;
                    case HideUICommand hideUI:
                        CommandGlobalReact(hideUI);
                        break;
                    case UIGroupCommand groupUI:
                        CommandGlobalReact(groupUI);
                        break;
                }
            }
        }

        public void CommandGlobalReact(ShowUIAndHideOthersCommand command)
        {
            HideAllExcept(command.UIActorType);
            EntityManager.Command(new ShowUICommand { UIViewType = command.UIActorType, OnUILoad = command.OnUILoad });
        }

        public void CommandGlobalReact(HideAllUIExceptCommand command)
        {
            HideAllExcept(command.UIActorType);
        }

        private void HideAllExcept(int uIActorType)
        {
            foreach (var e in uiCurrents.Data)
            {
                if (e == null || !e.IsAlive) continue;

                if (e.GetHECSComponent<UITagComponent>(ref uiTagComponentMask).ViewType.Id == uIActorType) continue;
                e.Command(new HideUICommand());
            }
        }

        public void CommandGlobalReact(UIGroupCommand command)
        {
            if (command.Show)
                ShowGroup(command);
            else
                HideGroup(command);
        }

        private void HideGroup(UIGroupCommand command)
        {
            if (!isLoaded || !isReady)
            {
                commandsQueue.Enqueue(command);
                return;
            }

            IEntity[] array = uiCurrents.Data;
            var count = uiCurrents.Count;

            for (int i = 0; i < count; i++)
            {
                if (array[i].TryGetHecsComponent(uiGroupTagMask, out UIGroupTagComponent uIGroupTagComponent))
                {
                    if (uIGroupTagComponent.IsHaveGroupIndex(command.UIGroup))
                        uIGroupTagComponent.Owner.Command(new HideUICommand());
                }
            }
        }

        private void ShowGroup(UIGroupCommand command)
        {
            if (!isLoaded || !isReady)
            {
                commandsQueue.Enqueue(command);
                return;
            }

            IEntity[] array = uiCurrents.Data;
            var count = uiCurrents.Count;

            for (int i = 0; i < count; i++)
            {
                if (array[i].TryGetHecsComponent(uiGroupTagMask, out UIGroupTagComponent uIGroupTagComponent))
                {
                    if (uIGroupTagComponent.IsHaveGroupIndex(command.UIGroup))
                        continue;
                    else
                        array[i].Command(new HideUICommand());
                }
                else
                    array[i].Command(new HideUICommand());
            }

            for (int i = 0; i < uIBluePrints.Count; i++)
            {
                try
                {
                    if (uIBluePrints[i].Groups.IsHaveGroupIndex(command.UIGroup)
                    && !uiCurrents.Any(x => x.GetHECSComponent<UITagComponent>(ref uiTagComponentMask).ViewType.Id == uIBluePrints[i].UIType.Id))
                    {
                        SpawnUIFromBluePrint(uIBluePrints[i], command.OnLoadUI);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }

    public interface IUISystem : ISystem,
        IReactGlobalCommand<ShowUICommand>,
        IReactGlobalCommand<HideUICommand>,
        IReactGlobalCommand<ShowUIAndHideOthersCommand>,
        IReactGlobalCommand<HideAllUIExceptCommand>,
        IReactGlobalCommand<UIGroupCommand>,
        IReactGlobalCommand<CanvasReadyCommand>
    { }
}