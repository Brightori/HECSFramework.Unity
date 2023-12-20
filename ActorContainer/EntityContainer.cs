using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HECSFramework.Core;
using Components;
using System;
using System.ComponentModel;
using IComponent = HECSFramework.Core.IComponent;


#if UNITY_EDITOR
using UnityEditor;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
#endif

namespace HECSFramework.Unity
{
    public class EntityContainer : ScriptableObject, IValidate
    {
        [SerializeField]
        protected ComponentsSystemsHolder holder = new ComponentsSystemsHolder();

        public virtual List<SystemBaseBluePrint> Systems => holder.systems;
        public virtual List<ComponentBluePrint> Components => holder.components;

        [SerializeField, HideInInspector]
        private int containerIndex;

        public int ContainerIndex => containerIndex;

        public string CachedName 
        {

            get 
            { 
                if (string.IsNullOrEmpty(cachedName))
                    cachedName = name;

                return cachedName;
            }
        }

        [NonSerialized]
        private string cachedName;

        protected bool isEditorTimeChanged;

        public virtual void OnEnable()
        {
            holder.Parent = this;

            if (string.IsNullOrEmpty(name))
                return;

            containerIndex = IndexGenerator.GenerateIndex(name);
        }

        public bool IsHaveComponentBlueprint<U>(U componentBluePrint) where U : ComponentBluePrint
        {
            foreach (var c in Components)
            {
                if (c.Equals(componentBluePrint))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// we check component only at this container, if u use actorreference container
        /// u should use TryGet functionality
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual bool IsHaveComponent<T>()
        {
            if (holder == null)
            {
                HECSDebug.LogError($"{name}.EntityContainer:holder == null");
                return false;
            }
            if (holder.components == null)
            {
                HECSDebug.LogError($"{name}.EntityContainer:components == null");
                return false;
            }
            
            return Components.Any(x =>
            {
                if (x == null || x.GetHECSComponent == null)
                {
                    HECSDebug.LogError($"{name}.EntityContainer:GetHECSComponent == null");
                    return false;
                }
                return x.GetHECSComponent is T;
            });
        }

        public bool IsHaveComponent<T>(T component)
        {
            return IsHaveComponent<T>();
        }

        public bool ContainsComponent(int componentTypeHashcode, bool checkOnlyThisContainer = false)
        {
            if (checkOnlyThisContainer)
            {
                return holder.components.Any(x => x.GetHECSComponent.GetTypeHashCode == componentTypeHashcode);
            }
            else
                return Components.Any(x => x.GetHECSComponent.GetTypeHashCode == componentTypeHashcode);
        }

        public virtual T GetComponent<T>() where T : IComponent
        {
            foreach (var c in holder.components)
            {
                if (c.GetHECSComponent is T needed)
                    return needed;
            }

            return default;
        }

        public virtual T GetComponentInstance<T>() where T : IComponent
        {
            foreach (var c in holder.components)
            {
                if (c.GetHECSComponent is T needed)
                    return (T)c.GetComponentInstance();
            }

            return default;
        }

        /// <summary>
        /// looking for blueprints typeHashCode not for components itself
        /// </summary>
        /// <param name="bluePrintTypeHashCode"></param>
        /// <returns></returns>
        public virtual bool IsHaveComponent(int bluePrintTypeHashCode)
        {
            return Components.Any(x => IndexGenerator.GetIndexForType(x.GetType()) == bluePrintTypeHashCode);
        }

        public bool IsHaveSystem(SystemBaseBluePrint systemBaseBluePrint) =>
            holder.systems.Any(x => x.GetSystem.GetTypeHashCode == systemBaseBluePrint.GetSystem.GetTypeHashCode);

        public virtual void Init(Entity entity, bool pure = false)
        {
            entity.AddComponent(new ActorContainerID { ID = CachedName });
            InitComponents(entity, holder.components, pure);
            InitSystems(entity, holder.systems, pure);
        }

        public virtual bool TryGetComponent<T>(Func<T, bool> func, out T result)
        {
            foreach (var component in holder.components)
            {
                if (component.GetHECSComponent is T needed && func(needed))
                {
                    result = needed;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public virtual bool TryGetComponent<T>(out T result)
        {
            foreach (var component in holder.components)
            {
                if (component.GetHECSComponent is T needed)
                {
                    result = needed;
                    return true;
                }
            }
            result = default;
            return false;
        }

        protected void InitComponents(Entity entity, List<ComponentBluePrint> components, bool pure = false)
        {
            foreach (var component in components)
            {
                if (component == null)
                {
                    Debug.LogAssertion("null component у " + name, this);
                    continue;
                }

                if (!pure && component.GetHECSComponent is IHaveActor && !(entity.ContainsMask<ActorProviderComponent>()))
                    continue;

                component.LoadFromData(component.GetOrAddComponent(entity));
            }
        }

        protected void InitSystems(Entity entity, List<SystemBaseBluePrint> systems, bool pure = false)
        {
            foreach (var system in systems)
            {
                if (system == null)
                {
                    Debug.LogAssertion("null system у " + name, this);
                    continue;
                }

                if (!pure && system.GetSystem is IHaveActor && !(entity.ContainsMask<ActorProviderComponent>()))
                    continue;

                entity.AddHecsSystem(TypesMap.GetSystemFromFactory(system.GetTypeIndex()));
            }
        }

        public IEnumerable<T> GetComponents<T>()
        {
            foreach (var c in Components)
            {
                if (c.GetHECSComponent is T needed)
                    yield return needed;
            }
        }

        public List<IComponent> GetComponentsInstances()
        {
            var list = new List<IComponent>(holder.components.Count);

            foreach (var c in holder.components)
            {
                if (c == null)
                {
                    Debug.LogAssertion("пустой компонент у " + name);
                    continue;
                }

                list.Add(Instantiate(c).GetHECSComponent);
            }


            return list;
        }

        public List<ISystem> GetSystemsInstances()
        {
            var list = new List<ISystem>(holder.systems.Count);

            foreach (var s in holder.systems)
                list.Add(Instantiate(s).GetSystem);

            return list;
        }

        public virtual bool IsValid()
        {
#if UNITY_EDITOR

            if (Components == null || Components.Any(x=> x == null))
            {
                Debug.LogError($"we have null component on {name}", this);
                return false;
            }

            if (Systems == null || Systems.Any(x=> x == null))
            {
                Debug.LogError($"we have null system on {name}", this);
                return false;
            }
#endif

            return true;
        }


        #region Editor

#if UNITY_EDITOR
        [Button(ButtonSizes.Large)]
        private void AddComponent()
        {
            if (Selection.objects.Count() > 1)
            {
                Debug.Log("Следует выбрать только один контейнер для добавления компонента.");
                return;
            }

            var window = EditorWindow.GetWindow<AddBluePrintWindow>();
            window.Init(new List<EntityContainer> { this }, TypeOfBluePrint.Component);
            OnEnable();
        }

        [Button(ButtonSizes.Large)]
        private void AddSystem()
        {
            if (Selection.objects.Count() > 1)
            {
                Debug.Log("Следует выбрать только один контейнер для добавления системы.");
                return;
            }

            var window = EditorWindow.GetWindow<AddBluePrintWindow>();
            window.Init(new List<EntityContainer> { this }, TypeOfBluePrint.System);
        }

        [Button(ButtonSizes.Large)]
        private void LoadPreset()
        {
            var window = EditorWindow.GetWindow<ChoosePresetActorWindow>();
            window.Init(this);
        }

        [Button(ButtonSizes.Large)]
        public void ShowDependencies()
        {
            var window = EditorWindow.GetWindow<ShowDependenciesWindow>();
            window.Init(this);
        }

        [Button(ButtonSizes.Large)]
        public void AddToHistory()
        {
            var path = Application.dataPath + "/BluePrints/History/" + this.name + "_" + $"{DateTime.UtcNow.ToFileTimeUtc()}" + ".history";
            var path2 = "Assets/" + "/BluePrints/History/" + this.name + "_" + $"{DateTime.UtcNow.ToFileTimeUtc()}" + ".history";
            var json = JsonUtility.ToJson(new History(this), true);
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        [Button(ButtonSizes.Large)]
        public void LoadFromHistory()
        {
            var getWindow = EditorWindow.GetWindow<LoadHistoriesWindow>();
            getWindow.Init(this);
        }


        [Button]
        public void Sort()
        {
            holder.SortComponents();
        }

        [Button(ButtonSizes.Large)]
        public void AddFeature()
        {
            EditorWindow.GetWindow<ImportFeatureWindow>().Init(this);
        }

        public T GetOrAddComponent<T>() where T:  class, IComponent, new()
        {
            if (IsHaveComponent<T>())
                return GetComponent<T>();

            var bpProvider = new BluePrintsProvider();
            AddComponent(new T());

            isEditorTimeChanged = true;
            return GetComponent<T>();
        }

        public void AddComponent<T>(T component) where T : class, IComponent, new()
        {
            var bpProvider = new BluePrintsProvider();
            var key = component.GetType();

            if (bpProvider.Components.TryGetValue(key, out var needed))
            {
                var componentNode = new ComponentBluePrintNode(key.Name, needed, new List<EntityContainer> { this });
                componentNode.AddBluePrint(component);
            }
            else
                Debug.LogError($"we dont have bp for {key.Name} mby u should codogen first");
        }

        /// <summary>
        /// this method remove blueprint of component, by component type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        public void RemoveComponentBluePrint<T>(T component) where T : IComponent => holder.RemoveComponentBluePrint(component);

        private void SortComponents()
        {
            holder.components.Sort((a, b) =>
            {
                if (a == null || b == null)
                    return 0;

                if (a.IsVisible)
                    return b.IsVisible ? 0 : 1;
                if (b.IsVisible)
                    return -1;
                return 0;
            });
        }

        public void AddComponent(ComponentBluePrint componentBluePrint)
        {
            holder.components.Add(componentBluePrint);
            SortComponents();
            EditorUtility.SetDirty(this);
            isEditorTimeChanged = true;
        }

        public void AddSystem(SystemBaseBluePrint systemBP)
        {
            holder.systems.Add(systemBP);
            EditorUtility.SetDirty(this);
            isEditorTimeChanged = true;
        }

        public void AddSystemToContainer<T>() where T : class, ISystem, new()
        {
            if (Systems.Any(x => x.GetSystem is T))
                return;

            var bpProvider = new BluePrintsProvider();
            var key = typeof(T);

            if (bpProvider.Systems.TryGetValue(key, out var needed))
            {
                var componentNode = new SystemBluePrintNode(key.Name, needed, new List<EntityContainer> { this });
                componentNode.AddBluePrint();
            }
            else
                Debug.LogError($"we dont have bp for {key.Name} mby u should codogen first");
        }

        /// <summary>
        /// не использовать это вне эдитор скриптов, это функционал для редактора
        /// </summary>
        public void Clear()
        {
            holder.components.Clear();
            holder.systems.Clear();
            holder.Parent = this;
            holder.ClearDeletedBluePrints();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void ClearDeletedBluePrints() => holder.ClearDeletedBluePrints();

        private void OnValidate()
        {
#if UNITY_EDITOR
            //if (Application.isPlaying || EntityManager.IsAlive)
            //    return;
            //holder.OnValidate(this);
#endif
        }
#endif
        #endregion
    }
}