using System;
using HECSFramework.Core;
using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HECSFramework.Unity
{
    [Serializable]
    public class UnityFloatModifier : BaseModifier<float>
    {
        [SerializeField]
        private float value;

        [SerializeField]
        private ModifierCalculationType calculationType;

        [SerializeField]
        private ModifierValueType modifierType;

        [SerializeField, ReadOnly]
        private string guid;

        [SerializeField, IdentifierDropDown(nameof(ModifierIdentifier))]
        private int modifierIdentifier;

        private Guid currentGuid;

        public override int ModifierID { get=> modifierIdentifier; set=> modifierIdentifier = value; }

        public override float GetValue { get => value; set => this.value = value; }
        public override ModifierCalculationType GetCalculationType { get => calculationType; set => calculationType = value; }
        public override ModifierValueType GetModifierType { get => modifierType; set => modifierType = value; }

        public override Guid ModifierGuid
        {
            get
            {
                if (currentGuid != Guid.Empty)
                    return currentGuid;

                if (string.IsNullOrEmpty(guid))
                {
                    currentGuid = Guid.NewGuid();
                    guid = currentGuid.ToString();
                }
                else
                {
                    currentGuid = new Guid(guid);
                    return currentGuid;
                }

                return currentGuid;
            }
            set
            {
                currentGuid = value;
            }
        }

        public override void Modify(ref float currentMod)
        {
            currentMod = ModifiersCalculation.GetResult(currentMod, GetValue, GetCalculationType, GetModifierType);
        }
    }
}