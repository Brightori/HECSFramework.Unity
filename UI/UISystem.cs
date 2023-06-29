using System;
using System.Collections.Generic;
using System.Linq;
using Commands;
using Components;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using HECSFramework.Unity;
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

        private EntitiesFilter uiCurrents;
        private EntitiesFilter additionalCanvases;

        private UnityTransformComponent mainCanvasTransform;
        private List<UIBluePrint> uIBluePrints = new List<UIBluePrint>();
        private PoolingSystem poolingSystem;

        private bool isReady;
        private bool isLoaded;

        [Required]
        public UIStackComponent UIStackComponent;

        private HashSet<UIBluePrint> spawnInProgress = new ();

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
        }

        public async void CommandGlobalReact(ShowUICommand command)
        {
            await ShowUI
               (command.UIViewType, command.OnUILoad, 
                command.MultyView, command.ShowPrevious, command.AdditionalCanvasID, 
                command.UIGroup, command.ClearStack, command.Poolable);
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

            Addressables.InstantiateAsync(spawn.UIActor, mainTransform).Completed += a => UILoaded(a, action);
        }

        public async UniTask<Entity> ShowUI
           (int uiViewType, Action<Entity> onLoadUI = null,  
            bool multiView = false, bool showPrevious = false, int additionalCanvas = 0,  
            int uiGroup = 0, bool clearStack = false, bool ispoolable = false, bool needInit = true)
        {
            if (!isLoaded || !isReady)
                await UniTask.WaitUntil(() => isReady && isLoaded, PlayerLoopTiming.LastEarlyUpdate);

            if (!multiView)
            {
                if (TryGetCurrentUI(uiViewType, out var ui))
                    return ui;
            }

            Transform canvas = null;

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

            var bluePrint = GetUIBluePrint(uiViewType);

            if (bluePrint == null)
                throw new Exception("we dont have blue print for this ui " + uiViewType);

            if (ispoolable)
            {
                var container = await poolingSystem.GetEntityContainerFromPool(bluePrint.Container);
                var uiActorFromPool = await poolingSystem.GetActorFromPool<UIActor>(container);
                
                if (needInit)
                {
                    uiActorFromPool.Entity.Init();
                    uiActorFromPool.GetHECSComponent<UnityTransformComponent>().Transform.SetParent(canvas);
                }
                  
                return uiActorFromPool.Entity;
            }

            var newUIactorPrfb = await Addressables.LoadAssetAsync<GameObject>(bluePrint.UIActor).Task;
            var newUiActor = MonoBehaviour.Instantiate(newUIactorPrfb, canvas).GetComponent<UIActor>();

            if (needInit)
                newUiActor.Init();
            else
                newUiActor.InitActorWithoutEntity();

            newUiActor.ActorContainer.Init(newUiActor.Entity);

            newUiActor.transform.SetParent(canvas);
            return newUiActor.Entity;
        }

        private UIBluePrint GetUIBluePrint(int uiType)
        {
            return uIBluePrints.FirstOrDefault(x => x.UIType.Id == uiType);
        }

        private bool TryGetCurrentUI(int uiType, out Entity uiEntity)
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

        private void UILoaded(AsyncOperationHandle<GameObject> obj, Action<Entity> onUILoad)
        {
            if (obj.Result.TryGetComponent<UIActor>(out var actor))
            {
                actor.InitWithContainer();
                actor.Command(new ShowUICommand());
                onUILoad?.Invoke(actor.Entity);
            }
            else
                Debug.LogAssertion("this is not UIActor " + obj.Result.name);
        }

        public async void CommandGlobalReact(HideUICommand command)
        {
            if (!isLoaded || !isReady)
                await UniTask.WaitUntil(() => isReady && isLoaded, PlayerLoopTiming.LastEarlyUpdate);

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

        }

        private void HideGroup(int groupID)
        {
            uiCurrents.ForceUpdateFilter();

            foreach (var ui in uiCurrents)
            {
                if (ui.TryGetComponent(out UIGroupTagComponent uIGroupTagComponent))
                {
                    if (uIGroupTagComponent.IsHaveGroupIndex(groupID))
                        uIGroupTagComponent.Owner.Command(new HideUICommand());
                }
            }
        }

        private void ShowGroup(int groupID)
        {
            uiCurrents.ForceUpdateFilter();

            foreach (var ui in uiCurrents)
            {
                if (ui.TryGetComponent(out UIGroupTagComponent uIGroupTagComponent))
                {
                    if (uIGroupTagComponent.IsHaveGroupIndex(groupID))
                        continue;
                    else
                        ui.Command(new HideUICommand());
                }
                else
                    ui.Command(new HideUICommand());
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
    }

    public interface IUISystem : ISystem,
        IReactGlobalCommand<ShowUICommand>,
        IReactGlobalCommand<HideUICommand>,
        IReactGlobalCommand<CanvasReadyCommand>
    { }
}