using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "ActorReferenceContainer", menuName = "Actor Reference Container")]
    [Documentation(Doc.HECS, "This is reference container for actor containers, if u need reference container for ur own containers, u should make another class with generic param")]
    public class ActorReferenceContainer : ReferenceContainerBase<ActorContainer>
    {
    }

    [Documentation(Doc.HECS, "This is base for all reference containers, reference container also contain references to other containers, and init entity from itself first and after init from references")]
    public abstract class ReferenceContainerBase<U> : ActorContainer, IReferenceContainer where U : EntityContainer
    {
        [PropertyOrder(-9)]
        public U[] References = default;

        [NonSerialized]
        private bool isInited;
        [NonSerialized]
        private List<ComponentBluePrint> componentsBluePrints = new List<ComponentBluePrint>(32);
        [NonSerialized]
        private List<SystemBaseBluePrint> systemBaseBluePrints = new List<SystemBaseBluePrint>(32);
        [NonSerialized]
        private HashSet<EntityContainer> entityContainersPassed = new HashSet<EntityContainer>(8);
        private List<RefContainerNode> refContainersOnQueue = new List<RefContainerNode>(8);

        private int maxGeneration;

        public override void Init(IEntity entity, bool pure = false)
        {
            if (!isInited)
            {
                CollectComponentAndSystems(this, 0);
                isInited = true;
                ProcessGenerations();
            }

            InitComponents(entity, componentsBluePrints, pure);
            InitSystems(entity, systemBaseBluePrints, pure);

            entity.AddOrReplaceComponent(new ActorContainerID { ID = name });
        }

        private void ProcessGenerations()
        {
            for (int i = 0; i < maxGeneration+1; i++)
            {
                foreach (var n in refContainersOnQueue)
                {
                    if (n.Generation == i)
                    {
                        ProcessComponents(n.EntityContainer);
                        ProcessSystems(n.EntityContainer);
                    }
                }
            }
        }

        private void CollectComponentAndSystems(EntityContainer entityContainer, int generation)
        {
            if (!entityContainersPassed.Contains(entityContainer))
            {
                entityContainersPassed.Add(entityContainer);
                refContainersOnQueue.Add(new RefContainerNode { EntityContainer = entityContainer, Generation = generation });
            }

            if (entityContainer is IReferenceContainer referenceContainer)
            {
                var nextGen = ++generation;
                if (maxGeneration < nextGen)
                    maxGeneration = nextGen;

                foreach (var container in referenceContainer.ReferenceContainers())
                {
                    CollectComponentAndSystems(container, nextGen);
                }
            }
        }

        private void ProcessComponents(EntityContainer entityContainer)
        {
            foreach (var c in entityContainer.Components)
            {
                if (!IsAlrdyContainsComponent(c))
                {
                    componentsBluePrints.Add(c);
                }
            }
        }

        private void ProcessSystems(EntityContainer entityContainer)
        {
            foreach (var s in entityContainer.Systems)
            {
                if (!IsAlrdyContainesSystem(s))
                {
                    systemBaseBluePrints.Add(s);
                }
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

        private bool IsAlrdyContainsComponent(ComponentBluePrint componentBluePrint)
        {
            var compType = componentBluePrint.GetType();

            foreach (var c in componentsBluePrints)
            {
                if (c.GetType() == compType)
                    return true;
            }

            return false;
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

        public IEnumerable<EntityContainer> ReferenceContainers() => References;
    }

    public struct RefContainerNode
    {
        public int Generation;
        public EntityContainer EntityContainer;
    }

    public interface IReferenceContainer
    {
        IEnumerable<EntityContainer> ReferenceContainers();
    }
}