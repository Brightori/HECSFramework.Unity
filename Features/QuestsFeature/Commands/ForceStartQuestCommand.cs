using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Quests, "we send this command when we want manualy start the quest, we can use it for starting quests from npc")]
    public struct ForceStartQuestCommand : ICommand
    {
        public QuestDataInfo QuestDataInfo;
    }
}