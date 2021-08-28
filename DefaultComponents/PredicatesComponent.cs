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
            Predicates.Clear();
            foreach (var p in predicatesBP)
            {
                if (p.GetPredicate is IInitable inited)
                    inited.Init();

                Predicates.Add(p.GetPredicate);
            }
        }

        partial void InitBeforeSync()
        {
            Init();
        }
    }
}