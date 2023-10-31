using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using Systems;
using UnityEngine;

namespace HECSFramework.Unity
{
    [DefaultExecutionOrder(-100)]
    public partial class Actor : MonoBehaviour, IEquatable<Actor>
    {
        [SerializeField, BoxGroup("Init")] private ActorInitModule actorInitModule = new ActorInitModule();
        [SerializeField, BoxGroup("Init")] private ActorContainer actorContainer;

        public GameObject GameObject => gameObject;
        public string ContainerID => Entity.ContainerID;
        public ActorContainer ActorContainer => actorContainer;

        [NonSerialized]
        public Entity Entity;

        public void Command<T>(T command) where T : struct, ICommand => Entity.Command(command);


        private void Awake()
        {
            if (actorInitModule.InitActorMode == InitActorMode.InitOnAwake)
                Init(EntityManager.Worlds[actorInitModule.WorldIndex], true, true);
        }


        /// <summary>
        /// standart init method, we init actor and entity by default, without container
        /// </summary>
        /// <param name="world"></param>
        /// <param name="initEntity"></param>
        /// <param name="initWithContainer"></param>
        public void Init(World world = null, bool initEntity = true, bool initWithContainer = false)
        {
            if (world == null)
                world = EntityManager.Default;

            if (Entity == null || !Entity.IsAlive)
            {
                Entity = world.GetEntityFromPool(gameObject.name);
                Entity.GUID = actorInitModule.Guid;
            }

            Entity.GetOrAddComponent<ActorProviderComponent>().Actor = this;
            Entity.GetOrAddComponent<UnityTransformComponent>();

            if (!Entity.IsInited)
            {
                if (initWithContainer)
                {
                    if (actorContainer != null)
                        actorContainer.Init(Entity);
                    else
                    {
                        HECSDebug.LogWarning("we try to init with empty container " + this.name);
                    }
                }

                if (initEntity)
                    Entity.Init();
            }
        }

        /// <summary>
        /// additional method to init actor 
        /// </summary>
        /// <param name="world"></param>
        public void InitActorWithoutEntity(World world = null)
        {
            Init(world, false, false);
        }

        /// <summary>
        /// here we init actor, entity, and entity container 
        /// </summary>
        /// <param name="world"></param>
        public void InitWithContainer(World world = null)
        {
            Init(world, true, true);
        }

        public void InitEntity()
        {
            Entity.Init();
        }

        protected virtual void Start()
        {
            if (actorInitModule.InitActorMode == InitActorMode.InitOnStart)
                Init(EntityManager.Worlds[actorInitModule.WorldIndex], true, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetHECSComponent<T>() where T : IComponent, new() => Entity.GetComponent<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetHECSComponent<T>(out T component)
            where T : IComponent, new() => Entity.TryGetComponent(out component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetOrAddHECSComponent<T>() where T : IComponent, new() => Entity.GetOrAddComponent<T>();

        public void Dispose()
        {
            Entity.Dispose();
            Entity = null;
        }

        public bool Equals(Entity other) => Entity.Equals(other);

        public void GenerateGuid()
        {
            Entity.GenerateGuid();
            actorInitModule.SetGuid(Entity.GUID);
        }

        private void OnDestroy()
        {
            if (Entity.IsAlive() && Entity.World != null && Entity.World.IsAlive && EntityManager.IsAlive)
                Entity.Dispose();

            Entity = null;
        }

        public bool TryGetComponent<T>(out T component, bool lookInChildsToo = false)
        {
            if (!Entity.IsAlive())
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

        public override int GetHashCode()
        {
            if (Entity != null)
                return Entity.Index;

            if (EntityManager.IsAlive && gameObject != null)
                return gameObject.GetHashCode();

            return -999;
        }

        public override bool Equals(object other)
        {
            return other is Actor otherActor && Equals(otherActor);
        }

        protected virtual void Reset()
        {
            actorInitModule.SetID(gameObject.name);
            actorInitModule.SetGuid(Guid.NewGuid());
        }

        public void HecsDestroy()
        {
            if (Entity != null && !Entity.IsDisposed)
                Entity.Dispose();

            Entity = null;
            Destroy(gameObject);
        }

        public void RemoveActorToPool()
        {
            Entity.World.GetSingleSystem<PoolingSystem>().Release(this);
            Entity?.Dispose();
            Entity = null;
        }

        public Entity InjectContainer(EntityContainer container, World world, bool isAdditive = false)
        {
            if (isAdditive)
                container.Init(Entity);
            else
            {
                Entity.Dispose();
                Entity = world.GetEntityFromPool(gameObject.name);
                Entity.GetOrAddComponent<ActorProviderComponent>().Actor = this;
                Entity.GetOrAddComponent<UnityTransformComponent>();
                container.Init(Entity);
            }

            return Entity;
        }

        public void InjectContainer(EntityContainer container, bool isAdditive = false)
        {
            InjectContainer(container, EntityManager.Default, isAdditive);
        }

        public void InjectContainer(EntityContainer container, World world, bool isAdditive = false, params IComponent[] additionalComponents)
        {

            var neededWorld = world == null ? EntityManager.Default : world;
            InjectContainer(container, neededWorld, isAdditive);

            if (additionalComponents != null && additionalComponents.Length > 0)
            {
                foreach (var additional in additionalComponents)
                    Entity.AddComponent(additional);
            }
        }

        /// <summary>
        /// we think both actor should be live in one world, otherwise u should check index of world too
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Actor other)
        {
            return other.IsAlive() && this.IsAlive() && other.Entity.ID == Entity.ID;
        }
    }
}