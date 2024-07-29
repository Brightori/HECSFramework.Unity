using System;
using HECSFramework.Core;

namespace Predicates
{
    [Serializable][Documentation(Doc.Predicates, Doc.Quests, "this predicate always return false, we add this to quests for starting them manualy")]
    public sealed class QuestManualStartPredicate : IPredicate
    {
        public bool IsReady(Entity target, Entity owner = null)
        {
            return false;
        }
    }
}