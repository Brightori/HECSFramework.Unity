using HECSFramework.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Components
{
    public abstract class StatComponent<T> : ModifiableFloatCounterComponent where T : IdentifierContainer
    {
        [SerializeField] protected float startValue = 10;
        [SerializeField] private T counterIdentifier;

        public override int Id => counterIdentifier.Id;
        public override float SetupValue => startValue;

        [ShowInInspector] //this property for debing|show at inspector
        public float ShowCurrentValue => modifiersContainer != null ? modifiersContainer.CurrentValue : startValue;

        
    }
}