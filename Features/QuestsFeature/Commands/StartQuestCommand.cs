using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Quests, "we send this command to quests entity when we want to start quest and make him active")]
	public struct StartQuestCommand : ICommand
	{
		public AliveEntity From;
		public AliveEntity To;
	}
}