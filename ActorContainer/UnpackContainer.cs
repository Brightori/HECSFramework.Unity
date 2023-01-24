using Components;
using HECSFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HECSFramework.Unity
{
    public struct UnpackContainer
    {
        public List<IComponent> Components;
        public List<ISystem> Systems;

        public UnpackContainer(EntityContainer entityContainer)
        {
            Components = new List<IComponent>(entityContainer.Components.Count);
            Systems = new List<ISystem>(entityContainer.Systems.Count);

            FillFromReferenceContainer(ref Components, entityContainer);

            foreach (var c in entityContainer.Components)
            {
                if (c != null)
                    Components.AddOrReplace(c);
            }

            foreach (var s in entityContainer.Systems)
                Systems.Add(MonoBehaviour.Instantiate(s).GetSystem);

            if (!Components.Any(x => x is ActorContainerID))
                Components.Add(new ActorContainerID { ID = entityContainer.name });

            CheckRefContainer(entityContainer);
        }

        private void FillFromReferenceContainer(ref List<IComponent> source, EntityContainer entityContainer)
        {
            if (entityContainer is ActorReferenceContainer referenceContainer)
            {
                foreach (var actoreContainer in referenceContainer.References)
                {
                    if (actoreContainer is ActorReferenceContainer recurciveContainer)
                    {
                        FillFromReferenceContainer(ref source, recurciveContainer);
                    }
                    if (actoreContainer != null)
                    {
                        foreach (var c in actoreContainer.Components)
                            source.AddOrReplace(c);
                    }
                }
            }
        }

        public void InitEntity(Entity entity)
        {
            if (entity.IsInited)
                throw new Exception("entity was alrdy inited " + entity.ID);

            foreach (var c in Components)
            {
                if (!entity.ContainsMask(c.GetTypeHashCode))
                {
                    entity.AddComponent(c);
                }
            }

            foreach (var s in Systems)
            {
                if (entity.Systems.Any(x => x.GetTypeHashCode == s.GetTypeHashCode))
                    continue;

                entity.AddHecsSystem(s);
            }
        }

        public T GetComponent<T>() where T : IComponent
        {
            return (T)Components.FirstOrDefault(x => x is T);
        }

        public T GetSystem<T>() where T : ISystem
        {
            return (T)Systems.FirstOrDefault(x => x is T);
        }

        private void CheckRefContainer(EntityContainer entityContainer)
        {
            if (!(entityContainer is ActorReferenceContainer referenceContainer)) return;

            foreach (var reference in referenceContainer.References)
            {
                foreach (var c in reference.Components)
                {
                    if (c != null)
                        Components.Add(MonoBehaviour.Instantiate(c).GetHECSComponent);
                }

                foreach (var s in reference.Systems)
                    Systems.Add(MonoBehaviour.Instantiate(s).GetSystem);
            }

        }
    }
    public static class IComponentListExt
    {
        public static void AddOrReplace(this IList<IComponent> source, ComponentBluePrint element)
        {
            var comp = element.GetHECSComponent;
            var indx = -1;
            for (var i = 0; i < source.Count; i++)
            {
                var x = source[i];
                if (x.GetTypeHashCode == comp.GetTypeHashCode)
                {
                    indx = i;
                    break;
                }
            }
            if (indx != -1)
                source.RemoveAt(indx);

            source.Add(MonoBehaviour.Instantiate(element).GetHECSComponent);
        }

    }
}