using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Components;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "ActorReferenceContainer", menuName = "Actor Reference Container")]
    [Documentation(Doc.HECS, "This is reference container for actor containers, if u need reference container for ur own containers, u should make another class with generic param")]
    public sealed class ActorReferenceContainer : ReferenceContainerBase<ActorContainer>
    {
    }

    [Documentation(Doc.HECS, "This is base for all reference containers, reference container also contain references to other containers, and init entity from itself first and after init from references")]
    public abstract class ReferenceContainerBase<U> : ActorContainer, IReferenceContainer where U : EntityContainer
    {
        [PropertyOrder(-9)]
        [OnValueChanged("RemoveReferences")]
        public U[] References = new U[0];

        [NonSerialized]
        private bool isInited;

        [NonSerialized]
        private List<ComponentBluePrint> componentsBluePrints = new List<ComponentBluePrint>(32);

        [NonSerialized]
        private List<SystemBaseBluePrint> systemBaseBluePrints = new List<SystemBaseBluePrint>(32);

        public override List<ComponentBluePrint> Components
        {
            get
            {
                InitActorReferenceContainer();
                return componentsBluePrints;
            }
        }

        public override List<SystemBaseBluePrint> Systems
        {
            get
            {
                InitActorReferenceContainer();
                return systemBaseBluePrints;
            }
        }

        public override void Init(Entity entity, bool pure = false)
        {
            InitActorReferenceContainer();

            InitComponents(entity, componentsBluePrints, pure);
            InitSystems(entity, systemBaseBluePrints, pure);

            entity.GetOrAddComponent<ActorContainerID>().ID = name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitActorReferenceContainer()
        {
            if (!isInited || isEditorTimeChanged)
            {
                foreach (var component in holder.components)
                {
                    componentsBluePrints.Add(component);
                }

                foreach (var system in holder.systems)
                {
                    systemBaseBluePrints.Add(system);
                }

                foreach (var r in References)
                {
                    foreach (var component in r.Components)
                    {
                        if (IsAlrdyContainsComponent(component))
                            continue;

                        componentsBluePrints.Add(component);
                    }

                    foreach (var system in r.Systems)
                    {
                        if (IsAlrdyContainesSystem(system))
                            continue;

                        systemBaseBluePrints.Add(system);
                    }
                }

                isInited = true;
                isEditorTimeChanged = false;
            }
        }

        private bool IsAlrdyContainesSystem(SystemBaseBluePrint systemBaseBluePrint)
        {
            var sysType = systemBaseBluePrint.GetType();

            foreach (var c in systemBaseBluePrints)
            {
                if (c.GetType() == sysType)
                    return true;
            }

            return false;
        }

        private void RemoveReferences()
        {
            isInited = false;
        }

        private bool IsAlrdyContainsComponent(ComponentBluePrint componentBluePrint)
        {
            if (componentBluePrint == null)
            {
                Debug.Log($"{name}: componentBluePrint == null");
                return false;
            }

            var compType = componentBluePrint.GetType();

            foreach (var c in componentsBluePrints)
            {
                if (c == null)
                    continue;

                if (c.GetType() == compType)
                    return true;
            }

            return false;
        }


        public override void OnEnable()
        {
            base.OnEnable();

            InitActorReferenceContainer();

            foreach (var componentBluePrint in Components)
            {
                componentBluePrint.IsColorNeeded = true;
                componentBluePrint.IsOverride = References.Any(a => a.IsHaveComponentBlueprint(componentBluePrint));
            }
        }

        public override T GetComponent<T>()
        {
            foreach (var c in Components)
            {
                if (c.GetHECSComponent is T needed)
                    return needed;
            }

            return default;
        }

        public override bool TryGetComponent<T>(Func<T, bool> func, out T result)
        {
            foreach (var component in componentsBluePrints)
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

        public override bool IsValid()
        {
            if (!base.IsValid())
                return false;

            foreach (var container in References)
            {
                if (!container.IsValid())
                    return false;
            }

            return true;
        }

        public override bool IsHaveComponent<T>()
        {
            foreach (var component in Components)
            {
                if (component.GetHECSComponent is T)
                    return true;
            }

            return false;
        }

        public override bool IsHaveComponent(int bluePrintTypeHashCode)
        {
            return holder.components.Any(x => IndexGenerator.GetIndexForType(x.GetType()) == bluePrintTypeHashCode);
        }

        public override bool TryGetComponent<T>(out T result)
        {
            foreach (var component in Components)
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

        public IEnumerable<EntityContainer> ReferenceContainers() => References;
    }

    public interface IReferenceContainer
    {
        IEnumerable<EntityContainer> ReferenceContainers();
    }
}