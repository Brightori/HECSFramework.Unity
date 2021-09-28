using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    [Serializable]
    public partial class PredicatesComponent : BaseComponent, IInitable
    {
        [SerializeField] private PredicateBluePrint[] predicatesBP = new PredicateBluePrint[0];

        public void Init()
        {
            if (Predicates.Count > 0)
                return;

            Predicates.Clear();
            foreach (var p in predicatesBP)
            {
                if (p.GetPredicate is IInitable inited)
                    inited.Init();

                Predicates.Add(p.GetPredicate);
            }
        }
    }
}