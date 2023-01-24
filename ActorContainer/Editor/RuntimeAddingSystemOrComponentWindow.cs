using System;
using System.Collections.Generic;
using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace HECSFramework.HECS.Unity.ActorContainer
{
    public class RuntimeAddingSystemOrComponentWindow : OdinEditorWindow
    {
        [Searchable, HideReferenceObjectPicker]
        public List<BaseNode> bluePrints = new(64);

        public void Init(List<Entity> entities, TypeOfBluePrint type)
        {
            bluePrints.Clear();
            var bluePrintProvider = new BluePrintsProvider();

            switch (type)
            {
                case TypeOfBluePrint.Component:
                    foreach (var t in bluePrintProvider.Components)
                        bluePrints.Add(new ComponentNode(t.Key.Name, t.Key, entities));
                    break;
                case TypeOfBluePrint.System:
                    foreach (var t in bluePrintProvider.Systems)
                        bluePrints.Add(new SystemNode(t.Key.Name, t.Key, entities));
                    break;
            }
        }
        
        public abstract class BaseNode
        {
            [HideLabel, ReadOnly]
            public string Name;

            protected Type neededType;
            protected List<Entity> entities;

            public BaseNode(string name, Type neededType, List<Entity> entities)
            {
                Name = name;
                this.neededType = neededType;
                this.entities = entities;
            }
            
            public abstract void Add();
        }

        [Serializable]
        public class ComponentNode : BaseNode
        {
            public ComponentNode(string name, Type neededComponent, List<Entity> entities) : base(
                name, neededComponent, entities)
            {
            }

            [Button("Add Component")]
            public override void Add()
            {
                var typeIndex = IndexGenerator.GetIndexForType(neededType);
                var component = TypesMap.GetComponentFromFactory(typeIndex);

                foreach (var entity in entities)
                {

                    if(entity.ContainsMask(typeIndex))
                        continue;
                    entity.AddComponent(component);
                }
                
                Name = component.GetType().Name;
            }
        }

        [Serializable]
        public class SystemNode : BaseNode
        {
            public SystemNode(string name, Type neededComponent, List<Entity> entities) : base(name,
                neededComponent, entities)
            {
            }

            [Button("Add System")]
            public override void Add()
            {
                var typeIndex = IndexGenerator.GetIndexForType(neededType);
                var system = TypesMap.GetSystemFromFactory(typeIndex);

                foreach (var parent in entities)
                {
                    parent.AddHecsSystem(system);
                }
                Name = system.GetType().Name;
            }

        }
    }
}