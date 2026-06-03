using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide collision from monobeh to actor")]
    public struct CollisionCommand : ICommand
    {
        public Collision Collision;
        public Entity Owner { get; set; }
        public Entity Target { get; set; }
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide collision from monobeh to actor, we also add index from our collider to associate collision with collider")]
    public struct CollisionIndexCommand : ICommand
    {
        public int Index;
        public Collision Collision;
        public Entity Owner { get; set; }
        public Entity Target { get; set; }
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide 2d collision from monobeh to actor")]
    public struct Collision2dCommand : ICommand
    {
        public Collision2D Collision;
        public Entity Owner { get; set; }
        public Entity Target { get; set; }
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide collision from monobeh to actor, we also add index from our collider to associate collision with collider")]
    public struct Collision2dIndexCommand : ICommand
    {
        public int Index;
        public Collision2D Collision;
        public Entity Owner { get; set; }
        public Entity Target { get; set; }
    }
}