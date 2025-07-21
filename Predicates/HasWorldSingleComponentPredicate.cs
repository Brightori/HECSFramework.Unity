using System;
using System.Collections;
using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;

namespace Predicates
{
    [Serializable]
    [Documentation(Doc.Predicates, "This predicate check if target world has single world component")]
    public sealed class HasWorldSingleComponentPredicate : IPredicate
    {
        private enum Contains
        {
            Contain,
            NotContain
        }
#if UNITY_2017_1_OR_NEWER
        [UnityEngine.SerializeField]
        [Sirenix.OdinInspector.PropertyOrder(2)]
#endif
        private Contains entityShouldContain = Contains.Contain;

#if UNITY_2017_1_OR_NEWER
        [Sirenix.OdinInspector.ReadOnly]
        [Sirenix.OdinInspector.PropertyOrder(-1)]
#endif
        public int Index;

        public bool IsReady(Entity target, Entity owner = null)
        {
            if (!TypesMap.GetComponentInfo(Index, out var info))
                return true;

            if (target == null)
                return true;

            switch (entityShouldContain)
            {
                case Contains.Contain:
                    return target.World.IsHaveSingleComponent(info.ComponentsMask.TypeHashCode);
                case Contains.NotContain:
                    return !target.World.IsHaveSingleComponent(info.ComponentsMask.TypeHashCode);
            }

            return true;
        }

        [ValueDropdown("SetIndexFromBP")]
        [LabelText("Component")]
        [ShowInInspector]
        public int SetIndex
        {
            get => Index;
            set => Index = value;
        }

        private IEnumerable SetIndexFromBP()
        {
            var bpProvider = new BluePrintsProvider();
            var list = new ValueDropdownList<int>();

            foreach (var bp in bpProvider.Components)
            {
                if (typeof(IWorldSingleComponent).IsAssignableFrom(bp.Key))
                {
                    list.Add(bp.Key.Name, IndexGenerator.GenerateIndex(bp.Key.Name));
                }
            }

            return list;
        }
    }
}