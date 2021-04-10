using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using Systems;

namespace Components
{
    [Serializable, BluePrint]
    public  class AbilityTagComponent : BaseComponent, IAbilityTagComponent
    {
        public IAbility Ability;
    }

    public interface IAbilityTagComponent : IComponent
    {
    }
}