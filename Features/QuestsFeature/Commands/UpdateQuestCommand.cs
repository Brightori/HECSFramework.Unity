using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Quests, "we send this command to active quests, for updating their states")]
    public struct UpdateQuestGlobalCommand : IGlobalCommand
    {
    }
}