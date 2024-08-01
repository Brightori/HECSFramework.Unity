using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Quests, "we send this command when we want manualy complete the quest, when we want use this command we should sure about quest entity can process it, if not - u will dont have result")]
    public struct ForceCompleteQuestCommand : IGlobalCommand
    {
        public QuestDataInfo QuestDataInfo;
        
        /// <summary>
        /// this is optional data, for cases with npc or with rewards
        /// </summary>
        public Entity From;
        public Entity To;
    }
}