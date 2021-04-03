using System;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
	[Serializable, BluePrint]
    public class StartSystem : BaseSystem, IStartSystem
    {
        public override void InitSystem()
        {
        }

        public bool Equals(ISystem other)
        {
            return other is IStartSystem;
        }

        public void StartGame()
        {
        }
    }

    public interface IStartSystem : ISystem
    {
        void StartGame();
    }
}