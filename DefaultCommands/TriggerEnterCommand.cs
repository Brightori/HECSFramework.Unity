using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
	[Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide trigger event from monobeh to actor")]
	public struct TriggerEnterCommand : ICommand
	{
		public Collider Collider;
	}

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide trigger event from monobeh to actor")]
    public struct TriggerEnterIndexCommand : ICommand
    {
		public int Index;
        public Collider Collider;
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide trigger exit event from monobeh to actor")]
	public struct TriggerExitCommand : ICommand
	{
		public Collider Collider;
	}

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide trigger exit event from monobeh to actor")]
    public struct TriggerExitIndexCommand : ICommand
    {
        public int Index;
        public Collider Collider;
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide trigger event from monobeh to actor")]
	public struct Trigger2dEnterCommand : ICommand
	{
		public Collider2D Collider;
	}

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide trigger event from monobeh to actor")]
    public struct Trigger2dEnterIndexCommand : ICommand
    {
        public int Index;
        public Collider2D Collider;
    }

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide trigger exit event from monobeh to actor")]
	public struct Trigger2dExitCommand : ICommand
	{
		public Collider2D Collider;
	}

    [Documentation(Doc.HECS, Doc.Physics, Doc.Provider, "Provide trigger exit event from monobeh to actor")]
    public struct Trigger2dExitIndexCommand : ICommand
    {
        public int Index;
        public Collider2D Collider;
    }
}