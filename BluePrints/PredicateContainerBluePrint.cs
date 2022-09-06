using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class PredicateContainerBluePrint : ScriptableObject
    {
        public abstract IPredicateEntityContainer GetPredicate { get; }
    }
}