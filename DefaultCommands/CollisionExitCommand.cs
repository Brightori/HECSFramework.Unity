using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
    public struct CollisionExitCommand : ICommand
    {
        public Collision Collision;
        public IEntity Owner { get; set; }
        public IEntity Target { get; set; }
    }
}