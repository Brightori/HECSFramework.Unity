using System;
using System.Linq;
using Components;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using Systems;
using UnityEngine;

namespace HECSFramework.Unity
{
    [DefaultExecutionOrder(-100)]
    public partial class Actor : MonoBehaviour, IActor
    {
        [SerializeField, BoxGroup("Init")] private ActorInitModule actorInitModule = new ActorInitModule();
        [SerializeField, BoxGroup("Init")] private ActorContainer actorContainer;

        private Entity entity;
        public GameObject GameObject => gameObject;

        public string ContainerID => entity.ContainerID;
        public ActorContainer ActorContainer => actorContainer;

        public Entity Entity => entity;

        public void Command<T>(T command) where T : struct, ICommand => entity.Command(command);

        private void Awake()
        {
            if (actorInitModule.InitActorMode == InitActorMode.InitOnStart)
                Init();
        }

        public void Init()
        {
            Init(EntityManager.Default);
            actorContainer.Init(entity);
        }

        public void Init(World world)
        {
            entity = world.PullEntity(gameObject.name);
            entity.GUID = actorInitModule.Guid;
            entity.GetOrAddComponent<ActorProviderComponent>().Actor = this;
        }

        protected virtual void Start()
        {
            if (actorInitModule.InitActorMode == InitActorMode.InitOnStart)
                Init();
        }

        public void Dispose()
        {
            entity.Dispose();
        }

        public bool Equals(Entity other) => entity.Equals(other);

        public void GenerateGuid()
        {
            entity.GenerateGuid();
            actorInitModule.SetGuid(entity.GUID);
        }

        private void OnDestroy()
        {
            if (entity.IsAlive)
                entity.Dispose();
        }

        public bool TryGetComponent<T>(out T component, bool lookInChildsToo = false)
        {
            if (!entity.IsAlive())
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
            return entity != null ? entity.GetHashCode() : gameObject.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return entity.Equals(other);
        }

        public void InjectContainer(EntityContainer container, bool isAdditive = false)
        {
            var components = container.GetComponentsInstances();
            var systems = container.GetSystemsInstances();

            entity.Inject(components, systems, isAdditive);
        }

        public void InjectContainer(EntityContainer container, bool isAdditive = false, params IComponent[] additionalComponents)
        {
            var components = container.GetComponentsInstances();
            var systems = container.GetSystemsInstances();

            foreach (var additional in additionalComponents)
                components.Add(additional);

            entity.Inject(components, systems, isAdditive);
        }

        protected virtual void Reset()
        {
            actorInitModule.SetID(gameObject.name);
            actorInitModule.SetGuid(Guid.NewGuid());
        }

        public void DestroyActorDisposeEntity()
        {
            throw new NotImplementedException();
        }

        public void RemoveActorToPool()
        {
            throw new NotImplementedException();
        }
    }
}