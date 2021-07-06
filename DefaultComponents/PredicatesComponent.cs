using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
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