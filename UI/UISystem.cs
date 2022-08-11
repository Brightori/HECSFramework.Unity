using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Systems
{
    [Serializable, BluePrint]
    [Documentation(Doc.UI, Doc.HECS, "This system default for operating ui at hecs, this system have command for show and hide ui plus show or hide ui groups, this system still in progress")]
    public class UISystem : BaseSystem, IUISystem, IGlobalStart
    {
        public const string UIBluePrints = "UIBluePrints";

        private Queue<IGlobalCommand> commandsQueue = new Queue<IGlobalCommand>();

        private ConcurrencyList<IEntity> uiCurrents;
        private ConcurrencyList<IEntity> additionalCanvases;

        private UnityTransformComponent mainCanvasTransform;
        private List<UIBluePrint> uIBluePrints = new List<UIBluePrint>();

        private HECSMask uiTagMask = HMasks.GetMask<UITagComponent>();
        private HECSMask additionalCanvasMask = HMasks.GetMask<AdditionalCanvasTagComponent>();
        private HECSMask uiGroupTagMask = HMasks.GetMask<UIGroupTagComponent>();
        private HECSMask mainCanvasTagComponentMask = HMasks.GetMask<MainCanvasTagComponent>();
        private HECSMask uiTagComponentMask = HMasks.GetMask<UITagComponent>();
        private HECSMask unityTransformMask = HMasks.GetMask<UnityTransformComponent>();

        private Systems.PoolingSystem poolingSystem;

        private bool isReady;
        private bool isLoaded;

        public override void InitSystem()
        {
            uiCurrents = EntityManager.Filter(new FilterMask(uiTagMask));
            additionalCanvases = EntityManager.Filter(new FilterMask(additionalCanvasMask));
            Addressables.LoadAssetsAsync<UIBluePrint>(UIBluePrints, null).Completed += LoadReact;
        }

        public void GlobalStart()
        {
            poolingSystem = Owner.World.GetSingleSystem<PoolingSystem>();
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
                Debug.LogAssertion("Cannot find UIBluePrint for " + command.UIViewType);
                return;
            }

            SpawnUIFromBluePrint(spawn, command.OnUILoad, mainCanvasTransform.Transform);
        }

        private void SpawnUIFromBluePrint(UIBluePrint spawn, Action<IEntity> action, Transform transform)
        {
            Addressables.InstantiateAsync(spawn.UIActor, transform).Completed += a => LoadUI(a, action);
        }

        public async Task<IEntity> ShowUI(int uiType, bool isMultiple = false, int additionalCanvas = 0, bool ispoolable = false)
        {
            while (!isReady)
                await Task.Delay(50);

            if (!isMultiple)
            {
                if (TryGetFromCurrentUI(uiType, out var ui))
                    return ui;
            }

            Transform canvas = null;

            if (additionalCanvas == 0)
                canvas = mainCanvasTransform.Transform;
            else
            {
                var needCanvas = additionalCanvases
                    .FirstOrDefault(x => x.GetHECSComponent<AdditionalCanvasTagComponent>
                        (ref additionalCanvasMask).AdditionalCanvasIdentifier.Id == additionalCanvas);

                if (needCanvas == null)
                    throw new Exception("We dont have additional canvas " + additionalCanvas);

                canvas = needCanvas.GetHECSComponent<UnityTransformComponent>(ref unityTransformMask).Transform;
            }

            var bluePrint = GetUIBluePrint(uiType);

            if (bluePrint == null)
                throw new Exception("we dont have blue print for this ui " + uiType);

            if (ispoolable)
            {
                var container = await poolingSystem.GetEntityContainerFromPool(bluePrint.Container);
                var uiActorFromPool = await poolingSystem.GetActorFromPool<UIActor>(bluePrint.UIActor, container);
                uiActorFromPool.Init();

                uiActorFromPool.GetUnityTransformComponent().Transform.SetParent(canvas);
                return uiActorFromPool;
            }

            var newUIactorPrfb = await Addressables.LoadAssetAsync<GameObject>(bluePrint.UIActor).Task;
            var newUiActor = MonoBehaviour.Instantiate(newUIactorPrfb, canvas).GetComponent<UIActor>();

            newUiActor.Init();
            newUiActor.GetUnityTransformComponent().Transform.SetParent(canvas);
            return newUiActor;
        }

        private UIBluePrint GetUIBluePrint(int uiType)
        {
            return uIBluePrints.FirstOrDefault(x => x.UIType.Id == uiType);
        }

        private bool TryGetFromCurrentUI(int uiType, out IEntity uiEntity)
        {
            uiEntity = null;

            foreach (var ui in uiCurrents)
            {
                if (ui == null || !ui.IsAlive)
                    continue;

                var uiTag = ui.GetHECSComponent<UITagComponent>(ref uiTagComponentMask);

                if (uiTag.ViewType.Id == uiType)
                {
                    uiEntity = uiTag.Owner;
                    return true;
                }
            }

            return false;
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
                        SpawnUIFromBluePrint(uIBluePrints[i], command.OnLoadUI, mainCanvasTransform.Transform);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void CommandGlobalReact(ShowUIOnAdditionalCommand command)
        {
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
                Debug.LogAssertion("We dont have ui bluePrint " + command.UIViewType);
                return;
            }

            var neededCanvas = additionalCanvases
                .FirstOrDefault(x => x.GetHECSComponent<AdditionalCanvasTagComponent>
                    (ref additionalCanvasMask).AdditionalCanvasIdentifier.Id == command.AdditionalCanvasID);

            if (neededCanvas == null)
            {
                HECSDebug.LogError("We dont have canvas with id " + command.AdditionalCanvasID);
                return;
            }

            if (neededCanvas.TryGetHecsComponent(unityTransformMask, out UnityTransformComponent unityTransformComponent))
                SpawnUIFromBluePrint(spawn, command.OnUILoad, unityTransformComponent.Transform);
            else
                HECSDebug.LogError("we dont have unityTransform on " + neededCanvas.ID);
        }


    }

    public interface IUISystem : ISystem,
        IReactGlobalCommand<ShowUICommand>,
        IReactGlobalCommand<ShowUIOnAdditionalCommand>,
        IReactGlobalCommand<HideUICommand>,
        IReactGlobalCommand<ShowUIAndHideOthersCommand>,
        IReactGlobalCommand<HideAllUIExceptCommand>,
        IReactGlobalCommand<UIGroupCommand>,
        IReactGlobalCommand<CanvasReadyCommand>
    { }
}