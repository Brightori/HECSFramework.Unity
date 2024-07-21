using System;
using HECSFramework.Core;

namespace Components
{
    [Feature("Quest")]
    [Serializable][Documentation(Doc.Quests, "we indicate here position of quests in overall quest hierarchy")]
    public sealed class QuestInfoComponent : BaseComponent
    {
        public QuestDataInfo QuestDataInfo;
    }
}