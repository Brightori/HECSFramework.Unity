using Components;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ICommandService EntityCommandService => entity.EntityCommandService;
        public int WorldId => entity.WorldId;
        public World World => entity.World;
        public Guid GUID => entity.GUID;
        public ref HECSMask ComponentsMask => ref entity.ComponentsMask;
        public List<ISystem> GetAllSystems => entity.GetAllSystems;
        public ComponentContext ComponentContext => entity.ComponentContext;
        public IComponent[] GetAllComponents => entity.GetAllComponents;

        public string ID => entity.ID;
        public bool IsInited => entity.IsInited;
        public bool IsAlive => entity.IsAlive;
        public bool IsPaused => entity.IsPaused;

        public string ContainerID => entity.ContainerID;

        public void AddHecsComponent(IComponent component, IEntity owner, bool silently = false)
        {
            entity.AddHecsComponent(component, this, silently);
        }

        public void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem 
        {
            this.entity.AddHecsSystem(system, this);
        } 

        public void Command<T>(T command) where T : ICommand => entity.Command(command);
        public bool ContainsMask(ref HECSMask mask) => entity.ContainsMask(ref mask);

        private void Awake()
        {
            actorInitModule.InitModule(this);
            entity = new Entity(actorInitModule.ID, actorInitModule.WorldIndex);
            entity.SetGuid(actorInitModule.Guid);

            if (actorContainer != null && !IsInited)
                actorContainer.Init(this);
        }

        protected virtual void Start()
        {
            if (actorInitModule.InitActorMode == InitActorMode.InitOnStart)
                Init();
        }

        public void Dispose()
        {
            HecsDestroy();
        }

        public bool Equals(IEntity other) => entity.Equals(other);

        public void GenerateGuid() => entity.GenerateGuid();

        public void HecsDestroy()
        {
            entity.HecsDestroy();
            EntityManager.RegisterEntity(this, false);
            Destroy(gameObject);
        }

        public void Init(bool needRegister = true) 
        {
            entity.SetWorld();
            entity.InitComponentsAndSystems(needRegister);
            
            if (needRegister)
                EntityManager.RegisterEntity(this, true);

            _ = GetOrAddComponent<TransformComponent>(this);
            entity.AfterInit();
        }

        public void Init(int worldIndex, bool needRegister = true)
        {
            (entity as IChangeWorldIndex).SetWorldIndex(worldIndex);
            Init(needRegister);
        }

        public void InjectEntity(IEntity entity, IEntity owner = null, bool additive = false) => this.entity.InjectEntity(entity, this, additive);

        public void Pause() => entity.Pause();
        public void RemoveHecsComponent(IComponent component) => entity.RemoveHecsComponent(component);
        public void RemoveHecsComponent(HECSMask component) => entity.RemoveHecsComponent(component);
        public void RemoveHecsSystem(ISystem system) => entity.RemoveHecsSystem(system);

        public bool TryGetComponent<T>(out T component, bool lookInChildsToo = false)
        {
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

        public T GetOrAddComponent<T>(IEntity owner = null) where T: class, IComponent => entity.GetOrAddComponent<T>(this);

        public void SetGuid(Guid guid)
        {
            actorInitModule.SetGuid(guid);
            entity.SetGuid(guid);
        }

        public void AddOrReplaceComponent(IComponent component, IEntity owner = null, bool silently = false)
        {
            entity.AddOrReplaceComponent(component, this, silently);
        }

        public bool ContainsMask<T>() where T : IComponent => entity.ContainsMask<T>();
        public void RemoveHecsComponent<T>() where T : IComponent => entity.RemoveHecsComponent<T>();

        public bool TryGetHecsComponent<T>(out T component) where T : IComponent => entity.TryGetHecsComponent<T>(out component);

        public IEnumerable<T> GetComponentsByType<T>() where T : IComponent => entity.GetComponentsByType<T>();
    }
}