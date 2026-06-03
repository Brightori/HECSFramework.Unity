using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide exit collision from monobeh to actor")]
    public struct CollisionExitCommand : ICommand
    {
        public Collision Collision;
        public Entity Owner { get; set; }
        public Entity Target { get; set; }
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide exit collision from monobeh to actor")]
    public struct Collision2dExitCommand : ICommand
    {
        public Collision2D Collision;
        public Entity Owner { get; set; }
        public Entity Target { get; set; }
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide exit collision from monobeh to actor")]
    public struct CollisionExitIndexCommand : ICommand
    {
        public int Index;
        public Collision Collision;
        public Entity Owner { get; set; }
        public Entity Target { get; set; }
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide exit collision from monobeh to actor")]
    public struct Collision2dExitIndexCommand : ICommand
    {
        public int Index;
        public Collision2D Collision;
        public Entity Owner { get; set; }
        public Entity Target { get; set; }
    }
}