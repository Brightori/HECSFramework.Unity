using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide collision from monobeh to actor")]
    public struct CollisionCommand : ICommand
    {
        public Collision Collision;
        public IEntity Owner { get; set; }
        public IEntity Target { get; set; }
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide 2d collision from monobeh to actor")]
    public struct Collision2dCommand : ICommand
    {
        public Collision2D Collision;
        public IEntity Owner { get; set; }
        public IEntity Target { get; set; }
    }
}