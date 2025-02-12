﻿using System.Collections;
using System.Linq;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector;

namespace Components
{
    public sealed partial class AbilitiesHolderComponent : BaseComponent
    {
        [ValueDropdown("GetAbilities", NumberOfItemsBeforeEnablingSearch = 3)]
        public EntityContainer[] AbilitiesContainers = new EntityContainer[0];

        private IEnumerable GetAbilities()
        {
            return new SOProvider<EntityContainer>().GetCollection().Where(x => x.IsHaveComponent<AbilityTagComponent>());
        }

        public void LoadDefaultAbilities()
        {
            foreach (var ability in AbilitiesContainers)
            {
                var newAbility = ability.GetEntity();

                if (ability.IsHaveComponent<InitOnAddAbilityTagComponent>())
                    AddAbility(newAbility, true);
                else
                    AddAvailableAbility(newAbility);
            }
        }
    }
}