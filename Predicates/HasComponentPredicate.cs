using System.Collections;
using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;

namespace Predicates
{
    public sealed partial class HasComponentPredicate : IPredicate
    {
        [ValueDropdown("SetIndexFromBP")]
        [LabelText("Component")]
        [ShowInInspector]
        public int SetIndex { get => Index; set=> Index = value; }

        private IEnumerable SetIndexFromBP()
        {
            var bpProvider = new BluePrintsProvider();
            var list = new ValueDropdownList<int>();

            foreach (var bp in bpProvider.Components)
            {
                list.Add(bp.Key.Name, IndexGenerator.GenerateIndex(bp.Key.Name));
            }

            return list;
        }
    }
}
