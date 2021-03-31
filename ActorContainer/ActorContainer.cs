using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HECSFramework.Core;
using Components;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "Actor Container", menuName = "Actor Container")]
    public class ActorContainer : ScriptableObject
    {
        [SerializeField]
        protected ComponentsSystemsHolder holder = new ComponentsSystemsHolder();

        public List<SystemBaseBluePrint> Systems => holder.systems;
        public List<ComponentBluePrint> Components => holder.components;

        public bool IsHaveComponentBlueprint<U>(U componentBluePrint) where U: ComponentBluePrint
        {
            return holder.components.Any(x => x is U);
        }

        public bool IsHaveComponent<T>()
        {
            return holder.components.Any(x => x.GetHECSComponent is T);
        }

        public bool IsHaveComponent<T>(T component)
        {
            return IsHaveComponent<T>();
        }

        public bool IsHaveSystem(SystemBaseBluePrint systemBaseBluePrint) =>
            holder.systems.Any(x => x.GetSystem.GetType().Name == systemBaseBluePrint.GetSystem.GetType().Name);

        public void Init(IEntity entity)
        {
            entity.AddHecsComponent(new ActorContainerID { ID = name });
            foreach (var component in holder.components)
            {
                if (component == null)
                {
                    Debug.LogAssertion("null component у " + name, this);
                    continue;
                }

                entity.AddHecsComponent(Instantiate(component).GetHECSComponent);
            }

            foreach (var system in holder.systems)
            {
                if (system == null)
                {
                    Debug.LogAssertion("null system у " + name, this);
                    continue;
                }

                entity.AddHecsSystem(Instantiate(system).GetSystem);
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
            window.Init(new List<ActorContainer>{this}, TypeOfBluePrint.Component);
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
            window.Init(new List<ActorContainer>{this}, TypeOfBluePrint.System);
        }
#endif
        [Button(ButtonSizes.Large)]
        private void LoadPreset()
        {
            var window = EditorWindow.GetWindow<ChoosePresetActorWindow>();
            window.Init(this);
        }

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
            => holder.OnValidate(this);
#endif
        #endregion
    }
}