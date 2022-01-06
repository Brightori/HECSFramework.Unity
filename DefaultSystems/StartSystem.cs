using System;
using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// this is core part, use partial class in ur project and localStart
    /// </summary>
	[Serializable, BluePrint] //todo add partial part to codogen
    public partial class StartSystem : BaseSystem, IStartSystem
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
            LocalStart();
        }

        partial void LocalStart();
    }

    public interface IStartSystem : ISystem
    {
        void StartGame();
    }
}