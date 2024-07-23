using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Feature(Doc.Quests)]
    [Documentation(Doc.Quests, "here we hold completed quests, if whole group was completed, we remove quests indexes from completed quests, if all groups at stage was completed, we remove their indexes from cm")]
    public sealed class QuestsHistoryComponent : BaseComponent
    {
        public HashSet<int> CompletedStages = new HashSet<int>();
        public HashSet<QuestGroupInfo> CompletedGroups = new HashSet<QuestGroupInfo>();
        public HashSet<QuestDataInfo> CompletedQuests = new HashSet<QuestDataInfo>();
    }
}