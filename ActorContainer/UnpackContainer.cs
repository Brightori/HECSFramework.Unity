using HECSFramework.Core;
using System;
using System.Collections.Generic;

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
                    Components.Add(c.GetHECSComponent);
            }

            foreach (var s in entityContainer.Systems)
                Systems.Add(s.GetSystem);
        }

        public void InitEntity(IEntity entity)
        {
            if (entity.IsInited)
                throw new Exception("entity was alrdy inited " + entity.ID);

            foreach (var c in Components)
                entity.AddHecsComponent(c);

            foreach (var s in Systems)
                entity.AddHecsSystem(s);
        }
    }
}