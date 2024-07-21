using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Feature(Doc.Quests)]
    [Documentation(Doc.Quests, "here we hold active quests and active hierarchy of quests")]
    public sealed class QuestsStateComponent : BaseComponent
    {
        public HashSet<int> ActiveStages = new HashSet<int>();
        public HashSet<GroupQuestInfo> ActiveGroups = new HashSet<GroupQuestInfo>();
        public HashSet<Entity> ActiveQuests = new HashSet<Entity>();
    }
}