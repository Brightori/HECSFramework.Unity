using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using HECSFramework.HECS.Unity.ActorContainer;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity.Editor
{
    public class ShowComponentsAndSystems : OdinEditorWindow
    {
        [ShowInInspector]
        private Actor select;

        [OdinSerialize]
        private ActorPresentation ActorPresentation;

        [MenuItem("HECS Options/Debug/Show components and systems from actor")]
        public static void ShowWindow()
        {
            GetWindow<ShowComponentsAndSystems>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            PickUpActor();
        }

        private void Update()
        {
            Repaint();
        }

        [Button("Pick up from selected actor")]
        private void PickUpActor()
        {
            select = Selection.activeGameObject.GetComponent<Actor>();

            if (select == null)
            {
                ShowNotification(new GUIContent("не выбран актор"));
                return;
            }

            ActorPresentation = new ActorPresentation(select.Entity);
            ActorPresentation.UpdateIfo();
        }
    }

    [Serializable]
    public struct ActorPresentation
    {
        [ReadOnly]
        public Guid Guid;

        [ReadOnly]
        public string ContainerID;

        [ReadOnly]
        public int EntityIndex;

        [ReadOnly]
        public int WorldIndex; 

        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false, CustomAddFunction = nameof(HandleAddingComponent), CustomRemoveElementFunction = nameof(HandleRemovingComponent))]
        public List<IComponent> Components;

        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false, CustomAddFunction = nameof(HandleAddingSystem), CustomRemoveElementFunction = nameof(HandleRemovingSystem))]
        public List<ISystem> Systems;

        public bool IsAlive;
        public bool IsInited;
        public bool IsPaused;

        private readonly Entity entity;

        public ActorPresentation(Entity entity)
        {
            this.entity = entity;
            Guid = entity.GUID;
            ContainerID = entity.ContainerID;
            EntityIndex = entity.Index;

            IsAlive = entity.IsAlive;
            IsInited = entity.IsInited;
            IsPaused = entity.IsPaused;

            Components = new List<IComponent>(16);
            Systems = new List<ISystem>(16);
            WorldIndex = entity.WorldId;
        }

        public void UpdateIfo()
        {
            UpdateComponents();
            UpdateSystems();
        }

        private void UpdateSystems()
        {
            Systems.Clear();
            foreach (var s in entity.Systems)
            {
                if (s != null)
                    Systems.Add(s);
            }
        }

        private void UpdateComponents()
        {
            Components.Clear();
            foreach (var c in entity.GetComponentsByType<IComponent>())
            {
                Components.Add(c);
            }
        }

        private void HandleAddingComponent(List<IComponent> list)
        {
            var window = EditorWindow.GetWindow<RuntimeAddingSystemOrComponentWindow>();
            window.Init(new List<Entity> { entity }, TypeOfBluePrint.Component);
            UpdateComponents();
        }
        private void HandleRemovingComponent(List<IComponent> list, IComponent componentToRemove)
        {
            list.Remove(componentToRemove);
            entity.RemoveComponent(componentToRemove);
        }
        private void HandleRemovingSystem(List<ISystem> list, ISystem systemToRemove)
        {
            list.Remove(systemToRemove);
            entity.RemoveHecsSystem(systemToRemove);
        }
        private void HandleAddingSystem(List<ISystem> list)
        {
            var window = EditorWindow.GetWindow<RuntimeAddingSystemOrComponentWindow>();
            window.Init(new List<Entity> { entity }, TypeOfBluePrint.System);
            UpdateSystems();
        }
    }
}