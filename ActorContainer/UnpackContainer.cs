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

            foreach (var c in entityContainer.Components)
            {
                if (c != null)
                    Components.Add(MonoBehaviour.Instantiate(c).GetHECSComponent);
            }

            foreach (var s in entityContainer.Systems)
                Systems.Add(MonoBehaviour.Instantiate(s).GetSystem);
        }

        public void InitEntity(IEntity entity)
        {
            if (entity.IsInited)
                throw new Exception("entity was alrdy inited " + entity.ID);

            foreach (var c in Components)
            {
                if (TypesMap.GetComponentInfo(c.GetTypeHashCode, out var mask))
                {
                    if (entity.GetAllComponents[mask.ComponentsMask.Index] == null)
                    {
                        entity.AddHecsComponent(c);
                    }
                }
            }

            foreach (var s in Systems)
            {
                if (entity.GetAllSystems.Any(x=> x.GetTypeHashCode == s.GetTypeHashCode))
                    continue;

                entity.AddHecsSystem(s);
            }
        }
    }
}