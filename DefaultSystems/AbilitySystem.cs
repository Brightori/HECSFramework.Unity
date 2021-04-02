using System;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
	[Serializable, BluePrint]
    public abstract class AbilitySystem : BaseSystem, IAbilitySystem
    {
        public abstract void Execute(IEntity owner = null, IEntity target = null);
    }

    public interface IAbilitySystem : ISystem { }
}