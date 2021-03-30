using HECSFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HECSFramework.Unity
{
    public partial class Actor : MonoBehaviour, IActor
    {
        private Entity entity;
        public GameObject GameObject => gameObject;
        public ICommandService EntityCommandService => entity.EntityCommandService;
        public int WorldId => entity.WorldId;
        public World World => entity.World;
        public Guid GUID => entity.GUID;
        public HECSMask ComponentsMask => entity.ComponentsMask;
        public List<ISystem> GetAllSystems => entity.GetAllSystems;
        public ComponentContext ComponentContext => entity.ComponentContext;
        public IComponent[] GetAllComponents => entity.GetAllComponents;
        
        public string ID { get; }
        public bool IsInited { get; }
        public bool IsAlive { get; }
        public bool IsPaused { get; }

        public void AddHecsComponent(IComponent component, bool silently = false) 
            => entity.AddHecsComponent(component, silently);

        public void AddHecsSystem<T>(T system) where T : ISystem => entity.AddHecsSystem(system);
        public void Command<T>(T command) where T : ICommand => entity.Command(command);
        public bool ContainsMask(ref HECSMask mask) => entity.ContainsMask(ref mask);

        private void Awake()
        {
            entity = new Entity(gameObject.name);
        }

        public void Dispose()
        {
            entity.Dispose();
        }

        public bool Equals(IEntity other) => entity.Equals(other);

        public void GenerateID() => entity.GenerateID();

        public void HecsDestroy()
        {
            entity.HecsDestroy();
            Destroy(gameObject);
        }

        public void Init() => entity.Init();

        public void Init(int worldIndex) => entity.Init(worldIndex);

        public void InjectEntity(IEntity entity, bool additive = false) => this.entity.InjectEntity(entity, additive);

        public void Pause()
        {
            entity.Pause();
        }

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
            entity.SetID(gameObject.name);
            GenerateID();
        }

        public bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent =>
            entity.TryGetHecsComponent(mask, out component);

        public bool TryGetSystem<T>(out T system) where T : ISystem => entity.TryGetSystem(out system);

        public void UnPause()
        {
            entity.UnPause();
        }

        T IEntity.GetOrAddComponent<T>() => entity.GetOrAddComponent<T>();
    }
}