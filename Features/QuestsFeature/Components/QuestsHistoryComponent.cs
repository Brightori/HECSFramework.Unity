using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Feature(Doc.Quests)]
    [Documentation(Doc.Quests, "here we hold completed quests, if whole group was completed, we remove quests indexes from completed quests, if all groups at stage was completed, we remove their indexes from cm")]
    public sealed partial class QuestsHistoryComponent : BaseComponent, ISavebleComponent, IDirty
    {
        [Field(0)]
        public HashSet<QuestStageInfo> CompletedStages = new HashSet<QuestStageInfo>();
        [Field(1)]
        public HashSet<QuestGroupInfo> CompletedGroups = new HashSet<QuestGroupInfo>();
        [Field(2)]
        public HashSet<QuestDataInfo> CompletedQuests = new HashSet<QuestDataInfo>();

        public bool IsDirty { get; set; }
    }
}