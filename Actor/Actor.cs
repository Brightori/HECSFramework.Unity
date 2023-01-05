using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using Systems;
using UnityEngine;

namespace HECSFramework.Unity
{
    [DefaultExecutionOrder(-100)]
    public partial class Actor : MonoBehaviour, IActor, IHECSDisable, IHECSEnable
    {
        [SerializeField, BoxGroup("Init")] private ActorInitModule actorInitModule = new ActorInitModule();
        [SerializeField, BoxGroup("Init")] private ActorContainer actorContainer;

        private Entity entity = new Entity();
        public GameObject GameObject => gameObject;
        public EntityLocalCommandService EntityCommandService => entity.EntityCommandService;
        public int WorldId => entity.WorldId;
        public World World => entity.World;
        public Guid GUID => entity.GUID;
        public List<ISystem> GetAllSystems => entity.GetAllSystems;
        public ComponentContext ComponentContext => entity.ComponentContext;
        public IComponent[] GetAllComponents => entity.GetAllComponents;

        public string ID => entity.ID;
        public bool IsInited => entity != null && entity.IsInited;
        public bool IsAlive => entity != null && entity.IsAlive;
        public bool IsPaused => entity != null && entity.IsPaused;
        private bool entityCreated;

        public string ContainerID => entity.ContainerID;
        public ActorContainer ActorContainer => actorContainer;

        public HECSMultiMask ComponentsMask => entity.ComponentsMask;

        public LocalComponentListenersService RegisterComponentListenersService => entity.RegisterComponentListenersService;

        private HECSMask actorContainerMask = HMasks.GetMask<ActorContainerID>();

        public T AddHecsComponent<T>(T component, IEntity owner, bool silently = false) where T : IComponent
        {
            if (!entityCreated)
                CreateEntity();

            return entity.AddHecsComponent(component, this, silently);
        }

        public void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem
        {
            this.entity.AddHecsSystem(system, this);
        }

        public void ResetActor()
        {
            entity = new Entity(gameObject.name);
            entity.SetGuid(Guid.NewGuid());
        }

        public void Command<T>(T command) where T : struct, ICommand => entity.Command(command);
        public bool ContainsMask(ref HECSMask mask) => entity.ContainsMask(ref mask);

        private void Awake()
        {
            actorInitModule.InitModule(this);

            if (actorInitModule.InitActorMode == InitActorMode.InitOnStart)
            {
                CreateEntity();

                if (actorContainer != null && !IsInited)
                    actorContainer.Init(this);
            }
        }

        private void CreateEntity()
        {
            if (entityCreated)
                return;

            entity.SetID(actorInitModule.ID);
            entity.SetWorld(actorInitModule.WorldIndex);
            entity.SetGuid(actorInitModule.Guid);
            entityCreated = true;
        }

        private void CreateEntity(World world)
        {
            if (entityCreated)
                return;

            entity.SetID(actorInitModule.ID);
            entity.SetWorld(world);
            entity.SetGuid(actorInitModule.Guid);
            entityCreated = true;
        }

        public void InitWithContainer()
        {
            Awake();
            Init();
        }

        public void InitWithContainer(ActorContainer entityContainer)
        {
            actorContainer = entityContainer;
            InitWithContainer();
        }

        public void Init(bool needRegister = true)
        {
            if (entity == null)
                CreateEntity();

            if (World == null)
                entity.SetWorld(0);

            if (!IsInited && actorContainer != null && !entity.ContainsMask(ref actorContainerMask))
                actorContainer.Init(this);

            entity.InitComponentsAndSystems();

            if (needRegister)
                EntityManager.RegisterEntity(this, true);

            GetOrAddComponent<TransformComponent>(this);
            GetOrAddComponent<UnityTransformComponent>(this);

            entity.AfterInit();

            if (needRegister)
            {
                for (int i = 0; i < ComponentsMask.CurrentIndexes.Count; i++)
                {
                    var c = entity.GetAllComponents[ComponentsMask.CurrentIndexes[i]];

                    if (c.IsRegistered)
                        continue;

                    World?.AddOrRemoveComponent(c, true);
                    TypesMap.RegisterComponent(c.ComponentsMask.Index, c.Owner, true);
                    c.SetIsRegistered();
                }
            }

            entity.SetIsInited();
        }

        public void Init(int worldIndex, bool needRegister = true)
        {
            if (!entityCreated)
                CreateEntity(EntityManager.Worlds.Data[worldIndex]);

            Init(needRegister);
        }

        protected virtual void Start()
        {
            if (actorInitModule.InitActorMode == InitActorMode.InitOnStart)
                Init();
        }

        public void Dispose()
        {
            entity.HecsDestroy();
        }

        public bool Equals(IEntity other) => entity.Equals(other);

        public void GenerateGuid()
        {
            entity.GenerateGuid();
            actorInitModule.SetGuid(entity.GUID);
        }

        public void HecsDestroy()
        {
            World.GetSingleSystem<DestroyEntityWorldSystem>().ProcessDeathOfActor(this);
        }

        private void OnDestroy()
        {
            if (IsAlive)
                entity.HecsDestroy();
        }

        public void EntityDestroy() => entity.HecsDestroy();



        public void InjectEntity(IEntity entity, IEntity owner = null, bool additive = false) => this.entity.InjectEntity(entity, this, additive);

        public void Pause() => entity.Pause();
        public void RemoveHecsComponent(IComponent component) => entity.RemoveHecsComponent(component);
        public void RemoveHecsComponent(HECSMask component) => entity.RemoveHecsComponent(component);
        public void RemoveHecsSystem(ISystem system) => entity.RemoveHecsSystem(system);

        public bool TryGetComponent<T>(out T component, bool lookInChildsToo = false)
        {
            if (!IsAlive)
            {
                component = default;
                return false;
            }

            if (lookInChildsToo)
            {
                component = GetComponentsInChildren<T>(true).FirstOrDefault();
                return component != null;
            }

            component = GetComponent<T>();
            return component != null && component.ToString() != "null";
        }

        public bool TryGetComponents<T>(out T[] components)
        {
            components = GetComponentsInChildren<T>(true);
            return components != null && components.Length > 0;
        }

        protected virtual void Reset()
        {
            actorInitModule.SetID(gameObject.name);
            actorInitModule.SetGuid(Guid.NewGuid());
        }

        public bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent =>
            entity.TryGetHecsComponent(mask, out component);

        public bool TryGetSystem<T>(out T system) where T : ISystem => entity.TryGetSystem(out system);

        public void UnPause()
        {
            entity.UnPause();
        }

        public T GetOrAddComponent<T>(IEntity owner = null) where T : class, IComponent => entity.GetOrAddComponent<T>(this);

        public void SetGuid(Guid guid)
        {
            actorInitModule.SetGuid(guid);
            entity?.SetGuid(guid);
        }

        public void AddOrReplaceComponent(IComponent component, IEntity owner = null, bool silently = false)
        {
            entity.AddOrReplaceComponent(component, this, silently);
        }

        public bool ContainsMask<T>() where T : IComponent => entity.ContainsMask<T>();
        public void RemoveHecsComponent<T>() where T : IComponent => entity.RemoveHecsComponent<T>();

        public bool TryGetHecsComponent<T>(out T component) where T : IComponent => entity.TryGetHecsComponent<T>(out component);

        public IEnumerable<T> GetComponentsByType<T>() => entity.GetComponentsByType<T>();

        public void SetWorldIndex(int index)
        {
            throw new NotImplementedException();
        }

        public bool ContainsMask(HECSMultiMask mask) => entity.ContainsMask(mask);

        public bool ContainsAnyFromMask(FilterMask mask) => entity.ContainsAnyFromMask(mask);

        public bool ContainsAnyFromMask(HECSMultiMask mask) => entity.ContainsAnyFromMask(mask);

        public bool ContainsMask(FilterMask mask) => entity.ContainsMask(mask);

        public override int GetHashCode()
        {
            return entity != null ? entity.GetHashCode() : gameObject.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return entity.Equals(other);
        }

        public bool RemoveHecsSystem<T>() where T : ISystem => entity.RemoveHecsSystem<T>();


        /// <summary>
        /// if u need actor on different world, u should set world before init from container
        /// </summary>
        /// <param name="world"></param>
        public void SetWorld(World world = null)
        {
            if (world == null)
                world = EntityManager.Default;

            if (entity == null)
                CreateEntity(world);

            entity.SetWorld(world);
        }

        public void MigrateEntityToWorld(World world, bool needInit = true)
        {
            entity.MigrateEntityToWorld(world, needInit);
        }

        public void InjectContainer(EntityContainer container, bool isAdditive = false)
        {
            var components = container.GetComponentsInstances();
            var systems = container.GetSystemsInstances();

            entity.Inject(components, systems, isAdditive, this);
        }

        public void InjectContainer(EntityContainer container, bool isAdditive = false, params IComponent[] additionalComponents)
        {
            var components = container.GetComponentsInstances();
            var systems = container.GetSystemsInstances();

            foreach (var additional in additionalComponents)
                components.Add(additional);

            entity.Inject(components, systems, isAdditive, this);
        }

        public void Inject(List<IComponent> components, List<ISystem> systems, bool isAdditive = false, IEntity owner = null)
        {
            entity.Inject(components, systems, isAdditive, this);
        }

        public void HECSDisable()
        {
            foreach (var ci in entity.ComponentsMask.CurrentIndexes)
            {
                TypesMap.RegisterComponent(ci, entity, false);
            }

            foreach (var s in entity.GetAllSystems)
            {
                if (s is IHECSDisable disable)
                {
                    disable.HECSDisable();
                }
            }

            entity.Pause();
            entity.World.RegisterEntity(this, false);
        }

        public void HECSEnable()
        {
            entity.UnPause();

            entity.World.RegisterEntity(this, true);

            foreach (var ci in entity.ComponentsMask.CurrentIndexes)
            {
                TypesMap.RegisterComponent(ci, entity, true);
            }

            foreach (var s in entity.GetAllSystems)
            {
                if (s is IHECSEnable enable)
                {
                    enable.HECSEnable();
                }
            }
        }

        public Entity ReplaceEntity(Entity entity)
        {
            if (entity.IsInited)
                throw new Exception("entity for replace should be not inited");

            entity.SetWorld(this.entity.World);
            entity.SetGuid(this.entity.GUID);

            Dispose();
            this.entity = entity;
            entity.ProcessSystemsAndComponentsWithActor(this);
            
            return entity;
        }
    }
}