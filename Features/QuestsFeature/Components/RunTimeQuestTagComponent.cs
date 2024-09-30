using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Quests, Doc.Tag, Doc.HECS, "some quests should be able to be completed only here and now, without saving progress, but maintaining the fact that the quest has been completed, we mark such quests with this component")]
    public sealed class RunTimeQuestTagComponent : BaseComponent
    {
    }
}