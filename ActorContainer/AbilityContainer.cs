using Components;
using HECSFramework.Core;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "AbilityContainer", menuName = "Ability Container")]
    public class AbilityContainer : EntityContainer 
    {
        public override void Init(IEntity entity)
        {
            base.Init(entity);
            entity.AddHecsComponent(new AbilityTagComponent());
        }
    }
}