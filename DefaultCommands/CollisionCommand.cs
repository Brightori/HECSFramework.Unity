using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
    public struct CollisionCommand : ICommand
    {
        public Collision Collision;
        public IEntity Owner { get; set; }
        public IEntity Target { get; set; }
    }
}