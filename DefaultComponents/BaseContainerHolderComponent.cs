using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.HECS, Doc.Holder, Doc.GameLogic, "entity containers provider")]
    public abstract class BaseContainerHolderComponent<T> : BaseComponent, IValidate where T : IComponent
    {
        [SerializeField]
        [ValueDropdown(nameof(GetContainers))]
        protected EntityContainer[] containers;

        public bool TryGetContainerByID(int index, out EntityContainer entityContainer)
        {
            entityContainer = containers.FirstOrDefault(x => x.ContainerIndex == index);
            return entityContainer != null;
        }

        private IEnumerable<EntityContainer> GetContainers()
        {
            var containers = new SOProvider<EntityContainer>().GetCollection().Where(x => x is not PresetContainer && x.IsHaveComponent<T>()
               && !x.ContainsComponent(ComponentProvider<IgnoreReferenceContainerTagComponent>.TypeIndex, true));

            return containers;
        }

        public EntityContainer GetFirstOrDefault()
        {
            return containers.FirstOrDefault();
        }

        [Button]
        public virtual bool IsValid()
        {
            containers = GetContainers().ToArray();
            return true;
        }
    }
}