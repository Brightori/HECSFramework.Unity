using Commands;
using Components;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;
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

        private EntitiesFilter uiCurrents;
        private EntitiesFilter additionalCanvases;

        private UnityTransformComponent mainCanvasTransform;
        private List<UIBluePrint> uIBluePrints = new List<UIBluePrint>();
        private PoolingSystem poolingSystem;

        private List<UniTask<Entity>> cached = new List<UniTask<Entity>>(8);

        private bool isReady;
        private bool isLoaded;

        public override void InitSystem()
        {
            uiCurrents = Owner.World.GetFilter<UITagComponent>();
            additionalCanvases = Owner.World.GetFilter<AdditionalCanvasTagComponent>();
            Addressables.LoadAssetsAsync<UIBluePrint>(UIBluePrints, null).Completed += LoadReact;
        }

        public void GlobalStart()
        {
            poolingSystem = Owner.World.GetSingleSystem<PoolingSystem>();

            if (Owner.World.TryGetSingleComponent(out MainCanvasTagComponent mainCanvasTagComponent))
            {
                isReady = true;
                mainCanvasTransform = mainCanvasTagComponent.Owner.GetOrAddComponent<UnityTransformComponent>();
            }
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

                    var uiTag = ui.GetComponent<UITagComponent>();

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

        private void SpawnUIFromBluePrint(UIBluePrint spawn, Action<Entity> action, Transform mainTransform)
        {
            if (spawn.AdditionalCanvasIdentifier != null)
            {
                var neededCanvas = Owner.World.GetFilter<AdditionalCanvasTagComponent>()
                     .FirstOrDefault(x => x.GetComponent<AdditionalCanvasTagComponent>()
                         .AdditionalCanvasIdentifier.Id == spawn.AdditionalCanvasIdentifier.Id);

                if (neededCanvas != null)
                    mainTransform = neededCanvas.GetOrAddComponent<UnityTransformComponent>().Transform;
            }

            //todo rewrite  to use AssetServiceSystem
            Addressables.InstantiateAsync(spawn.UIActor, mainTransform).Completed += a => LoadUI(a, action);
        }

        public async UniTask<Entity> ShowUI(int uiType, bool isMultiple = false, int additionalCanvas = 0, bool needInit = true, bool ispoolable = false, bool isLocked = true)
        {
            while (!isReady)
                await Task.Delay(50);

            uiCurrents.ForceUpdateFilter();

            if (!isMultiple)
            {
                if (TryGetFromCurrentUI(uiType, out var ui))
                    return ui;
            }

            if (isLocked)
            {
                await new WaitRemove<UIBusyTagComponent>(Owner).RunJob();
                Owner.GetOrAddComponent<UIBusyTagComponent>();
            }

            Transform canvas = null;

            var bluePrint = GetUIBluePrint(uiType);

            if (bluePrint == null)
                throw new Exception("we dont have blue print for this ui " + uiType);

            if (additionalCanvas == 0 && bluePrint.AdditionalCanvasIdentifier != null)
            {
                additionalCanvas = bluePrint.AdditionalCanvasIdentifier.Id;
            }

            if (additionalCanvas == 0)
                canvas = mainCanvasTransform.Transform;
            else
            {
                var needCanvas = additionalCanvases
                    .FirstOrDefault(x => x.GetComponent<AdditionalCanvasTagComponent>().AdditionalCanvasIdentifier.Id == additionalCanvas);

                if (needCanvas == null)
                    throw new Exception("We dont have additional canvas " + additionalCanvas);

                canvas = needCanvas.GetComponent<UnityTransformComponent>().Transform;
            }

            if (ispoolable)
            {
                var container = await poolingSystem.GetEntityContainerFromPool(bluePrint.Container);
                var uiActorFromPool = await poolingSystem.GetActorFromPool<UIActor>(container);
                uiActorFromPool.Entity.Init();
                uiActorFromPool.GetHECSComponent<UnityTransformComponent>().Transform.SetParent(canvas);
                return uiActorFromPool.Entity;
            }

            var newUIactorPrfb = await Addressables.LoadAssetAsync<GameObject>(bluePrint.UIActor).Task;
            var newUiActor = MonoBehaviour.Instantiate(newUIactorPrfb, canvas).GetComponent<UIActor>();

            newUiActor.InitActorWithoutEntity();
            newUiActor.ActorContainer.Init(newUiActor.Entity);

            if (needInit)
                newUiActor.Init();

            newUiActor.transform.SetParent(canvas);

            if (isLocked)
                Owner.RemoveComponent<UIBusyTagComponent>();

            return newUiActor.Entity;
        }

        private UIBluePrint GetUIBluePrint(int uiType)
        {
            return uIBluePrints.FirstOrDefault(x => x.UIType.Id == uiType);
        }

        private bool TryGetFromCurrentUI(int uiType, out Entity uiEntity)
        {
            uiEntity = null;

            foreach (var ui in uiCurrents)
            {
                if (ui == null || !ui.IsAlive)
                    continue;

                var uiTag = ui.GetComponent<UITagComponent>();

                if (uiTag.ViewType.Id == uiType)
                {
                    uiEntity = uiTag.Owner;
                    return true;
                }
            }

            return false;
        }

        private void LoadUI(AsyncOperationHandle<GameObject> obj, Action<Entity> onUILoad)
        {
            if (obj.Result.TryGetComponent<UIActor>(out var actor))
            {
                actor.InitWithContainer();
                actor.Command(new ShowUICommand());
                onUILoad?.Invoke(actor.Entity);

                if (actor.Entity.TryGetComponent(out UIPriorityIndexComponent uIPriorityIndexComponent))
                {
                    SortUI(uIPriorityIndexComponent);
                }
            }
            else
                Debug.LogAssertion("this is not UIActor " + obj.Result.name);
        }

        private void SortUI(UIPriorityIndexComponent uIPriorityIndexComponent)
        {
            var transformComponent = uIPriorityIndexComponent.Owner.GetComponent<UnityTransformComponent>();
            var parent = transformComponent.Transform.parent;
            transformComponent.Transform.SetAsFirstSibling();

            for (int i = 1; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).TryGetComponent(out Actor actor))
                {
                    if (!actor.Entity.IsAlive() || actor.Entity == transformComponent.Owner)
                        continue;

                    if (actor.Entity.TryGetComponent(out UIPriorityIndexComponent currentPriority))
                    {
                        if (currentPriority.Priority < uIPriorityIndexComponent.Priority)
                        {
                            transformComponent.Transform.SetSiblingIndex(i);
                        }
                        else
                            return;
                    }
                    else
                    {
                        transformComponent.Transform.SetSiblingIndex(i);
                    }
                }
            }

            transformComponent.Transform.GetSiblingIndex();
        }

        public void CommandGlobalReact(HideUICommand command)
        {
            if (!isLoaded && !isReady)
            {
                commandsQueue.Enqueue(command);
                return;
            }

            uiCurrents.ForceUpdateFilter();
            foreach (var ui in uiCurrents)
            {
                if (ui == null || !ui.IsAlive)
                    continue;

                var uiTag = ui.GetComponent<UITagComponent>();

                if (uiTag.ViewType.Id == command.UIViewType)
                {
                    uiTag.Owner.Command(command);
                    return;
                }
            }
        }

        public void CommandGlobalReact(CanvasReadyCommand command)
        {
            if (EntityManager.TryGetSingleComponent<MainCanvasTagComponent>(Owner.WorldId, out var canvas))
            {
                if (!canvas.Owner.TryGetComponent(out mainCanvasTransform))
                    Debug.LogAssertion("we dont have unity transform on main canvas");
            }
            else
                Debug.LogAssertion("we dont have main canvas");

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

            foreach (var ui in uiCurrents)
            {
                if (ui.TryGetComponent(out UIGroupTagComponent uIGroupTagComponent))
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

            uiCurrents.ForceUpdateFilter();
            ShowUIGroup(command).Forget();
        }

        public async UniTask ShowUIGroup(UIGroupCommand command)
        {
            if (!isLoaded || !isReady)
            {
                commandsQueue.Enqueue(command);
                return;
            }

            await new WaitRemove<UIBusyTagComponent>(Owner).RunJob();

            Owner.GetOrAddComponent<UIBusyTagComponent>();

            foreach (var ui in uiCurrents)
            {
                if (ui.TryGetComponent(out UIGroupTagComponent uIGroupTagComponent))
                {
                    if (uIGroupTagComponent.IsHaveGroupIndex(command.UIGroup))
                    {
                        ui.Command(new ShowUICommand());
                        continue;
                    }

                    else
                        ui.Command(new HideUICommand());
                }
                else
                    ui.Command(new HideUICommand());
            }

            for (int i = 0; i < uIBluePrints.Count; i++)
            {
                if (uIBluePrints[i].Groups.IsHaveGroupIndex(command.UIGroup)
                && !IsCurrentUIContainsId(uIBluePrints[i].UIType.Id))
                {
                    cached.Add(ShowUI(uIBluePrints[i].UIType.Id, isLocked: false));
                }
            }

            await UniTask.WhenAll(cached);
            cached.Clear();

            command.OnLoadUI?.Invoke();
            Owner.RemoveComponent<UIBusyTagComponent>();
        }

        public HECSPooledArray<Entity> GetGroup(int index)
        {
            uiCurrents.ForceUpdateFilter();

            var result = HECSPooledArray<Entity>.GetArray(uiCurrents.Count);
            {
                foreach (var bp in uIBluePrints)
                {
                    if (bp.Groups.IsHaveGroupIndex(index))
                    {
                        var needed = uiCurrents.FirstOrDefault(x => x.GetComponent<UITagComponent>().ViewType.Id == bp.UIType.Id);

                        if (needed != null)
                            result.Add(needed);
                    }
                }

                return result;
            }
        }

        private bool IsCurrentUIContainsId(int id)
        {
            foreach (var ui in uiCurrents)
            {
                if (ui.GetComponent<UITagComponent>().ViewType.Id == id)
                    return true;
            }

            return false;
        }

        public void CommandGlobalReact(ShowUIOnAdditionalCommand command)
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

                    var uiTag = ui.GetComponent<UITagComponent>();

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
                .FirstOrDefault(x => x.GetComponent<AdditionalCanvasTagComponent>()
                    .AdditionalCanvasIdentifier.Id == command.AdditionalCanvasID);

            if (neededCanvas == null)
            {
                HECSDebug.LogError("We dont have canvas with id " + command.AdditionalCanvasID);
                return;
            }

            if (neededCanvas.TryGetComponent(out UnityTransformComponent unityTransformComponent))
                SpawnUIFromBluePrint(spawn, command.OnUILoad, unityTransformComponent.Transform);
            else
                HECSDebug.LogError("we dont have unityTransform on " + neededCanvas.ID);
        }
    }

    public interface IUISystem : ISystem,
        IReactGlobalCommand<ShowUICommand>,
        IReactGlobalCommand<HideUICommand>,
        IReactGlobalCommand<UIGroupCommand>,
        IReactGlobalCommand<CanvasReadyCommand>
    { }
}