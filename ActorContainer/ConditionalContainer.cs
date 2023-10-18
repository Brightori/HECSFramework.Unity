using System;
using System.Linq;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HECSFramework.Unity
{
    [Documentation(Doc.HECS, Doc.Containers, "this container can inject containers depends from predicates, attention pls - we not overlook is have components in conditionals containers ")]
    [Documentation(Doc.HECS, Doc.Containers, "we inject first container with all predicates ready")]
    [CreateAssetMenu(fileName = "ConditionalContainer", menuName = "ConditionalContainer")]
    public class ConditionalContainer : ActorContainer
    {
        [PropertyOrder(-1)]
        public ConditionsAndContainer[] ConditionsAndContainers;

        public override void Init(Entity entity, bool pure = false)
        {
            foreach (var condition in ConditionsAndContainers)
            {
                if (condition.Predicates.All(x => x.GetPredicate.IsReady(entity)))
                {
                    condition.Container.Init(entity, pure);
                    break;
                }
            }

            base.Init(entity, pure);
        }
    }

    [Serializable]
    public struct ConditionsAndContainer
    {
        public PredicateBluePrint[] Predicates;
        public EntityContainer Container;
    }
}
