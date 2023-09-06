using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    public sealed partial class AdditionalAbilityIndexComponent : BaseComponent, IInitable
    {
        [UnityEngine.SerializeField]
        private AdditionalAbilityIdentifier[] additionalAbilityIdentifiers = new AdditionalAbilityIdentifier[0];

        public void Init()
        {
            foreach (var identifier in additionalAbilityIdentifiers)
            {
                AdditionalIndeces.Add(identifier.Id);
            }
        }
    }
}
