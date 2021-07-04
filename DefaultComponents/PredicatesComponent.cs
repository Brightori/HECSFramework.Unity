using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    public partial class PredicatesComponent : BaseComponent, IInitable
    {
        [SerializeField] private PredicateBluePrint[] predicatesBP = new PredicateBluePrint[0];

        public void Init()
        {
            foreach (var p in predicatesBP)
            {
                predicates.Add(p.GetPredicate);
            }
        }
    }
}