#if UNITY_EDITOR
using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity
{
    public class AddBluePrintWindow : OdinEditorWindow
    {
        [Searchable, HideReferenceObjectPicker]
        public List<ActorContainerNode> bluePrints = new List<ActorContainerNode>(64);

        public void Init(List<EntityContainer> actorContainers, TypeOfBluePrint typeOfBluePrint)
        {
            bluePrints.Clear();
            var bluePrintProvider = new BluePrintsProvider();

            switch (typeOfBluePrint)
            {
                case TypeOfBluePrint.Component:
                    foreach (var t in bluePrintProvider.Components)
                        bluePrints.Add(new ComponentBluePrintNode(t.Key.Name, t.Value, actorContainers));
                    break;
                case TypeOfBluePrint.System:
                    foreach (var t in bluePrintProvider.Systems)
                        bluePrints.Add(new SystemBluePrintNode(t.Key.Name, t.Value, actorContainers));
                    break;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AssetDatabase.SaveAssets();
        }
    }

    public enum TypeOfBluePrint { Component, System }

    public abstract class ActorContainerNode
    {
        [HideLabel, ReadOnly]
        public string Name;

        protected Type neededType;
        protected List<EntityContainer> containers;

        public ActorContainerNode(string name, Type neededType, List<EntityContainer> containers)
        {
            Name = name;
            this.neededType = neededType;
            this.containers = containers;
        }
        
        public abstract void AddBluePrint();
    }

    [Serializable]
    public class ComponentBluePrintNode : ActorContainerNode
    {
        public ComponentBluePrintNode(string name, Type neededComponent, List<EntityContainer> containers) : base(name, neededComponent, containers)
        {
        }

        [Button("Add Component")]
        public override void AddBluePrint()
        {
            foreach (EntityContainer parent in containers)
            {
                var asset = ScriptableObject.CreateInstance(neededType);
                AssetDatabase.AddObjectToAsset(asset, parent);
                asset.name = neededType.Name;
                parent.AddComponent(asset as ComponentBluePrint);
            }
        }
    }
    
    [Serializable]
    public class SystemBluePrintNode : ActorContainerNode
    {
        public SystemBluePrintNode(string name, Type neededComponent, List<EntityContainer> containers) : base(name, neededComponent, containers)
        {
        }

        [Button("Add System")]
        public override void AddBluePrint()
        {
            var asset = ScriptableObject.CreateInstance(neededType);
            foreach (EntityContainer parent in containers)
            {
                AssetDatabase.AddObjectToAsset(asset, parent);
                asset.name = neededType.Name;
                parent.AddSystem(asset as SystemBaseBluePrint);
            }
        }
    }
}

#endif