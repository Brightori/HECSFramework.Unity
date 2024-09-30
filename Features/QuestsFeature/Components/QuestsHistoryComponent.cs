using System;
using System.Collections.Generic;
using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;

namespace Components
{
    [Serializable]
    [Feature(Doc.Quests)]
    [Documentation(Doc.Quests, "here we hold completed quests, if whole group was completed, we remove quests indexes from completed quests, if all groups at stage was completed, we remove their indexes from cm")]
    public sealed partial class QuestsHistoryComponent : BaseComponent, ISavebleComponent, IDirty, IWorldSingleComponent
    {
        [IdentifierDropDown(nameof(CounterIdentifierContainer))]
        public int Identifier;

        [Field(0)]
        public HashSet<QuestStageInfo> CompletedStages = new HashSet<QuestStageInfo>();
        [Field(1)]
        public HashSet<QuestGroupInfo> CompletedGroups = new HashSet<QuestGroupInfo>();
        [Field(2)]
        public HashSet<QuestDataInfo> CompletedQuests = new HashSet<QuestDataInfo>();

        public bool IsCompletedQuest(QuestDataInfo questDataInfo)
        {
            if (CompletedQuests.Contains(questDataInfo)) return true;
            if (CompletedGroups.Contains(questDataInfo)) return true;
            if (CompletedStages.Contains(questDataInfo)) return true;

            return false;
        }

        public bool IsDirty { get; set; }
    }
}