using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Quests, "we send this command to quests entity when we want to start quest and make him active ")]
	public struct StartQuestCommand : ICommand
	{
	}

    [Documentation(Doc.Quests, "we send this command to active quests, for updating their states")]
    public struct UpdateQuestCommand : ICommand
    {
    }

    [Documentation(Doc.Quests, "we send this command from ")]
    public struct QuestComplete : IGlobalCommand
    {
        public Entity Quest;
    }
}