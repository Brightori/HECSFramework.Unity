using HECSFramework.Core;
using HECSFramework.Unity;
using System;

namespace Components
{
    [Serializable, BluePrint]
    public class AbilityTagComponent : BaseComponent, IAbilityTagComponent
    {
       
    }

    public interface IAbilityTagComponent : IComponent
    {
    }
}