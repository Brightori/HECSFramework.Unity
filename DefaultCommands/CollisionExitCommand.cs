using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide exit collision from monobeh to actor")]
    public struct CollisionExitCommand : ICommand
    {
        public Collision Collision;
        public IEntity Owner { get; set; }
        public IEntity Target { get; set; }
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide exit collision from monobeh to actor")]
    public struct Collision2dExitCommand : ICommand
    {
        public Collision2D Collision;
        public IEntity Owner { get; set; }
        public IEntity Target { get; set; }
    }
}