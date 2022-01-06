using System;
using System.Linq;
using Components;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "ActorReferenceContainer", menuName = "Actor Reference Container")]
    [Documentation(Doc.GameLogic, "Это референс контейнер который инитит референс контейнеры, потом заменят инфу своими данными")]
    public class ActorReferenceContainer : ActorContainer
    {
        [PropertyOrder(-9)]
        public ActorContainer[] References = default;

        public override void Init(IEntity entity, bool pure = false)
        {
            if (References != null && References.Length > 0)
            {
                foreach (var reference in References)
                {
                    reference.Init(entity, pure);
                }
            }

            entity.AddOrReplaceComponent(new ActorContainerID { ID = name });
            
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

                entity.AddOrReplaceComponent(unpackComponent, entity);
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

        public override bool IsHaveComponent<T>()
        {
            return base.IsHaveComponent<T>() || References.Any(x => x.IsHaveComponent<T>());
        }

        public override void OnEnable()
        {
            base.OnEnable(); 
            foreach (var componentBluePrint in Components)
            {
                componentBluePrint.IsColorNeeded = true;
                componentBluePrint.IsOverride = References.Any(a => a.IsHaveComponentBlueprint(componentBluePrint));
            }
        }
        
        public override T GetComponent<T>()
        {
            foreach (var c in holder.components)
            {
                if (c.GetHECSComponent is T needed)
                    return needed;
            }

            foreach (var reference in References)
            {
                var component = reference.GetComponent<T>();
                if (component != null) return component;
            }
            
            return default;
        }
    }
    
}