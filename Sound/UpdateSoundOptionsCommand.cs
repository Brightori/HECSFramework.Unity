using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Sound, "Raise this to change Sound Settings")]
	public struct UpdateSoundOptionsCommand : IGlobalCommand
	{
		public bool IsSoundOn;
	}
}