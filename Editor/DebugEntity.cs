﻿using HECSFramework.Core;
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

        [OdinSerialize, SerializeField, InlineEditor]
        private List<IComponent> components = new List<IComponent>(16);

        [OdinSerialize, SerializeField, InlineEditor]
        private List<ISystem> systems = new List<ISystem>(16);

        
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

        [Button("Pick up from selected actor")]
        private void PickUpActor()
        {
            components.Clear();
            systems.Clear();

            select = Selection.activeGameObject.GetComponent<IActor>();

            if (select == null)
            {
                ShowNotification(new GUIContent("не выбран актор"));
                return;
            }

            foreach (var c in select.GetAllComponents)
                components.Add(c);

            foreach (var s in select.GetAllSystems)
                systems.Add(s);
        }
    }
}