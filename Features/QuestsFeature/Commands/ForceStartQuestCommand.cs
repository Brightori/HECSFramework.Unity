using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Quests, "we send this command when we want manualy start the quest, we can use it for starting quests from npc")]
    public struct ForceStartQuestCommand : IGlobalCommand
    {
        public QuestDataInfo QuestDataInfo;

        /// <summary>
        /// this is optional data, for cases with npc or with rewards
        /// </summary>
        public Entity From;
        public Entity To;
    }
}