using System;
using Components;
using HECSFramework.Core;
using UnityEngine;

namespace Systems
{
    [Serializable][Documentation(Doc.Timers, Doc.HECS, "this global system process timers holders and update timers")]
    public sealed class TimersGlobalSystem : BaseSystem, IPriorityUpdatable
    {
        private EntitiesFilter filter;

        public int Priority { get; } = -1;

        public override void InitSystem()
        {
            filter = Owner.World.GetFilter<TimersHolderComponent>();
        }

        public void PriorityUpdateLocal()
        {
            foreach (var e in filter)
            {
                e.GetComponent<TimersHolderComponent>().UpdateAllTimers(Time.deltaTime);
            }
        }
    }
}