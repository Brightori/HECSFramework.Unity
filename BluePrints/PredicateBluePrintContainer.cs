using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class PredicateBluePrintContainer<T> : PredicateBluePrint where T : IPredicate, new()
    {
        [SerializeField] private T predicate = new T();

        public override IPredicate GetPredicate => predicate;
    }
}