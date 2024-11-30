using System;
using Components;
using HECSFramework.Core;
using Helpers;

namespace Predicates
{
    [Serializable][Documentation(Doc.Timers, Doc.HECS, "this predicate checks in timers")]
    public sealed class IsHaveTimerPredicate : IPredicate
    {
        [IdentifierDropDown(nameof(TimerIdentifier))]
        public int TimerIndex;

        public bool IsReady(Entity target, Entity owner = null)
        {
            if (target.TryGetComponent(out TimersHolderComponent timersHolderComponent))
                return timersHolderComponent.IsContainsTimer(TimerIndex);

            return true;
        }
    }
}