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
    public partial class Actor : MonoBehaviour
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

        public void Init(World world = null, bool initEntity = true, bool initWithContainer = false)
        {
            if (world == null)
                world = EntityManager.Default;

            if (Entity == null)
            {
                Entity = world.GetEntityFromPool(gameObject.name);
                Entity.GUID = actorInitModule.Guid;
                Entity.GetOrAddComponent<ActorProviderComponent>().Actor = this;
            }
          
            if (!Entity.IsInited)
            {
                if (initWithContainer)
                {
                    actorContainer.Init(Entity);
                }

                if (initEntity)
                    Entity.Init();
            }
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

        public void Dispose()
        {
            Entity.Dispose();
        }

        public bool Equals(Entity other) => Entity.Equals(other);

        public void GenerateGuid()
        {
            Entity.GenerateGuid();
            actorInitModule.SetGuid(Entity.GUID);
        }

        private void OnDestroy()
        {
            if (Entity.IsAlive && EntityManager.IsAlive)
                Entity.Dispose();
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
            return Entity != null ? Entity.GetHashCode() : gameObject.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return Entity.Equals(other);
        }

       

        protected virtual void Reset()
        {
            actorInitModule.SetID(gameObject.name);
            actorInitModule.SetGuid(Guid.NewGuid());
        }

        public void HecsDestroy()
        {
            Entity.Dispose();
            Destroy(gameObject);
        }

        public void RemoveActorToPool()
        {
            Entity.World.GetSingleSystem<PoolingSystem>().ReturnActorToPool(this);
        }

        public Entity InjectContainer(EntityContainer container, World world, bool isAdditive = false)
        {
            if (isAdditive)
                container.Init(Entity);
            else
            {
                Entity.Dispose();
                Entity = world.GetEntityFromPool(gameObject.name);
                container.Init(Entity);
            }

            return Entity;
        }

        public void InjectContainer(EntityContainer container, bool isAdditive = false)
        {
            var components = container.GetComponentsInstances();
            var systems = container.GetSystemsInstances();

            Entity.Inject(components, systems, isAdditive);
        }

        public void InjectContainer(EntityContainer container, bool isAdditive = false, params IComponent[] additionalComponents)
        {
            var components = container.GetComponentsInstances();
            var systems = container.GetSystemsInstances();

            foreach (var additional in additionalComponents)
                components.Add(additional);

            Entity.Inject(components, systems, isAdditive);
        }
    }
}