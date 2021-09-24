using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity.Editor
{
    public class ShowComponentsAndSystems : OdinEditorWindow
    {
        [ShowInInspector]
        private IActor select;

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
            select = Selection.activeGameObject.GetComponent<IActor>();

            if (select == null)
            {
                ShowNotification(new GUIContent("не выбран актор"));
                return;
            }

             ActorPresentation = new ActorPresentation(select);
        }
    }

    [Serializable]
    public struct ActorPresentation
    {
        public Guid Guid;
        public string ContainerID;

        [ListDrawerSettings(Expanded = true, ShowPaging =false)]
        public List<IComponent> Components;

        [ListDrawerSettings(Expanded = true, ShowPaging = false)]
        public List<ISystem> Systems;

        public bool IsAlive;
        public bool IsInited;
        public bool IsPaused;

        public ActorPresentation (IEntity entity)
        {
            Guid = entity.GUID;
            ContainerID = entity.ContainerID;
            
            IsAlive = entity.IsAlive;
            IsInited = entity.IsInited;
            IsPaused = entity.IsPaused;

            Components = new List<IComponent>(16);
            Systems = new List<ISystem>(16);

            foreach (var c in entity.GetAllComponents)
            {
                if (c != null)
                    Components.Add(c);
            }

            foreach (var s in entity.GetAllSystems)
            {
                if (s != null)
                    Systems.Add(s);
            }
        }
    }
}