using HECSFramework.Core;

namespace HECSFramework.Unity
{
    public abstract class PredicateBluePrint : UnityEngine.ScriptableObject, IPredicateContainer
    {
        public abstract IPredicate GetPredicate { get; }
    }

    public abstract class PredicateBluePrintContainer<T> : PredicateBluePrint where T : IPredicate, new()
    {
        [UnityEngine.SerializeField] private T predicate = new T();

        public override IPredicate GetPredicate => predicate;
    }
}