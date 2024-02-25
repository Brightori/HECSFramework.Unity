using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    public sealed partial class AdditionalAbilityIndexComponent : BaseComponent 
    {
        [UnityEngine.SerializeField]
        private AdditionalAbilityIdentifier[] additionalAbilityIdentifiers = new AdditionalAbilityIdentifier[0];

        public override void Init()
        {
            foreach (var identifier in additionalAbilityIdentifiers)
            {
                AdditionalIndeces.Add(identifier.Id);
            }
        }
    }
}
