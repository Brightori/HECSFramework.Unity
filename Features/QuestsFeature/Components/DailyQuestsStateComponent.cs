using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Feature(Doc.Quests)]
    [Documentation(Doc.Quests, Doc.DailyQuests, "here we hold active daily quests and active hierarchy of quests")]
    public sealed class DailyQuestsStateComponent : BaseComponent
    {
        public HashSet<int> ActiveStages = new HashSet<int>();
        public HashSet<int> ActiveGroups = new HashSet<int>();
        public HashSet<Entity> ActiveQuests = new HashSet<Entity>();
    }
}