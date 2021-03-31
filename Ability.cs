using HECSFramework.Core;
using System.Collections.Generic;
using UnityEngine;

namespace HECSFramework.Unity
{
    public interface IHavePredicateContainers
    {
        List<PredicateBluePrint> Predicates { get; }
    }

    public interface IAbilityContainer
    {
        List<AbilityBaseBluePrint> AbilitiesBP { get; }
    }

    public abstract class AbilityBaseBluePrint : ActorContainer
    {
        public abstract IAbility GetAbility { get; }
    }

    public abstract class AbilityContainerBluePrint<T> : AbilityBaseBluePrint, IHavePredicateContainers where T : IAbility, new()
    {
        [SerializeField] private List<PredicateBluePrint> predicateContainers = new List<PredicateBluePrint>();
        [SerializeField] private T Ability = new T();

        public override IAbility GetAbility
        {
            get
            {
                var abil = Instantiate(this).Ability;
                abil.SetupAbilityData(GetComponentsInstances(), GetSystemsInstances(), GetPredicatesInstances());
                return abil;
            }
        }

        public List<PredicateBluePrint> Predicates => predicateContainers;

        public List<IPredicate> GetPredicatesInstances()
        {
            var list = new List<IPredicate>(predicateContainers.Count);

            foreach (var p in predicateContainers)
            {
                if (p != null)
                    list.Add(Instantiate(p).GetPredicate);
                else
                    Debug.LogAssertion("имеется null predicate " + name);
            }

            return list;
        }
    }
}