using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class PredicateBluePrint : ScriptableObject, IPredicateContainer
    {
        public abstract IPredicate GetPredicate { get; }
    }
}