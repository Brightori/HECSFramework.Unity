using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HECSFramework.Core;
using Components;
using System;


#if UNITY_EDITOR
using UnityEditor;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
#endif

namespace HECSFramework.Unity
{
    public class EntityContainer : ScriptableObject
    {
        [SerializeField]
        protected ComponentsSystemsHolder holder = new ComponentsSystemsHolder();

        public List<SystemBaseBluePrint> Systems => holder.systems;
        public List<ComponentBluePrint> Components => holder.components;

        public int ContainerIndex => IndexGenerator.GenerateIndex(name);

        public virtual void OnEnable()
        {
            holder.Parent = this;
        }

        public bool IsHaveComponentBlueprint<U>(U componentBluePrint) where U : ComponentBluePrint
        {
            return holder.components.Any(x => x.GetHECSComponent.GetTypeHashCode == componentBluePrint.GetHECSComponent.GetTypeHashCode);
        }

        public virtual bool IsHaveComponent<T>()
        {
            return holder.components.Any(x => x.GetHECSComponent is T);
        }

        public bool IsHaveComponent<T>(T component)
        {
            return IsHaveComponent<T>();
        }

        public virtual T GetComponent<T>() where T: IComponent
        {
            foreach (var c in holder.components)
            {
                if (c.GetHECSComponent is T needed)
                    return needed;
            }

            return default;
        }

        /// <summary>
        /// looking for blueprints typeHashCode not for components itself
        /// </summary>
        /// <param name="bluePrintTypeHashCode"></param>
        /// <returns></returns>
        public bool IsHaveComponent(int bluePrintTypeHashCode)
        {
            return Components.Any(x => IndexGenerator.GetIndexForType(x.GetType()) == bluePrintTypeHashCode);
        }

        public bool IsHaveSystem(SystemBaseBluePrint systemBaseBluePrint) =>
            holder.systems.Any(x => x.GetSystem.GetTypeHashCode == systemBaseBluePrint.GetSystem.GetTypeHashCode);

        public virtual void Init(IEntity entity, bool pure = false)
        {
            entity.AddHecsComponent(new ActorContainerID { ID = name });
            foreach (var component in holder.components)
            {
                if (component == null)
                {
                    Debug.LogAssertion("null component у " + name, this);
                    continue;
                }

                var unpackComponent = Instantiate(component).GetHECSComponent;

                if (!pure && unpackComponent is IHaveActor && !(entity is IActor actor))
                    continue;

                entity.AddHecsComponent(unpackComponent, entity);
            }

            foreach (var system in holder.systems)
            {
                if (system == null)
                {
                    Debug.LogAssertion("null system у " + name, this);
                    continue;
                }

                var unpackSys = Instantiate(system).GetSystem;

                if (!pure && unpackSys is IHaveActor && !(entity is IActor actor))
                    continue;

                entity.AddHecsSystem(unpackSys, entity);
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

        #region Editor

#if UNITY_EDITOR
#if DeveloperMode || ModifyMode
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
            var path = Application.dataPath + "/BluePrints/History/" + "___" + this.name + "___" + $"{DateTime.Now.ToString("u").Replace(" ", "_").Replace(":", "-")}" + ".history";
            var json = JsonUtility.ToJson(new History(this), true);
            System.IO.File.WriteAllText(path, json);
        }

        [Button(ButtonSizes.Large)]
        public void LoadFromHistory()
        {
            var getWindow = EditorWindow.GetWindow<LoadHistoriesWindow>();
            getWindow.Init(this);
        }

       
#endif

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
        }

        public void AddSystem(SystemBaseBluePrint systemBP)
        {
            holder.systems.Add(systemBP);
            EditorUtility.SetDirty(this);
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

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying || EntityManager.IsAlive)
                return;
            holder.OnValidate(this);
#endif
        }

#endif
        #endregion
    }
}