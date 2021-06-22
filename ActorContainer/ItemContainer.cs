using Components;
using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "ItemContainer", menuName = "Item Container")]
    public class ItemContainer : EntityContainer 
    {
        public override void Init(IEntity entity)
        {
            base.Init(entity);
            entity.GetOrAddComponent<ItemTagComponent>();
        }
    }
}