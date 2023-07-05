using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Components
{
    public abstract class StatComponent<T> : ModifiableFloatCounterComponent, IBaseValue<float> where T : IdentifierContainer
    {
        [SerializeField] protected float startValue = 10;
        [SerializeField] private T counterIdentifier;

        public override int Id => counterIdentifier.Id;
        public override float SetupValue => startValue;
        public float GetBaseValue => startValue;

        [ShowInInspector] //this property for debing|show at inspector
        public float ShowCurrentValue => Value;

        public void SetupBaseValue(float newBaseValue)
        {
            modifiableFloatCounter.Setup(Id, SetupValue);
        }
    }
}