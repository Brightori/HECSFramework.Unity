using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Quests, "we send this command from quest when quest was complete")]
    public struct QuestCompleteGlobalCommand : IGlobalCommand
    {
        public Entity Quest;
    }
}