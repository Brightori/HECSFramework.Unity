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
        public HashSet<QuestGroupInfo> ActiveGroups = new HashSet<QuestGroupInfo>();
        public HashSet<Entity> ActiveQuests = new HashSet<Entity>();
    }
}